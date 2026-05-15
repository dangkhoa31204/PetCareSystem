using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCareSystem.API.Dtos.Customer;
using PetCareSystem.API.Enums;
using PetCareSystem.API.Models;

namespace PetCareSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MembershipController : ControllerBase
{
    private readonly PetCareSystemContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MembershipController(PetCareSystemContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// [Public] Lấy danh sách các gói membership đang hoạt động
    /// </summary>
    [HttpGet("plans")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMembershipPlans()
    {
        var plans = _context.MembershipPlans.Where(p => p.IsActive == true).ToList();

        var result = plans.Select(p => new MembershipPlanDto
        {
            MembershipPlanId = p.MembershipPlanId,
            Name = p.Name,
            Price = p.Price,
            DurationDays = p.DurationDays,
            Description = p.Description,
            IsActive = p.IsActive
        }).ToList();

        return Ok(result);
    }

    /// <summary>
    /// [Customer] Lấy thông tin gói membership hiện tại của người dùng
    /// </summary>
    [HttpGet("my-membership")]
    public async Task<IActionResult> GetMyMembership()
    {
        var userId = long.Parse(_httpContextAccessor.HttpContext?.User.FindFirst("UserId")?.Value ?? "0");

        var membership = _context.UserMemberships
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .FirstOrDefault();

        if (membership == null)
        {
            return Ok(new { message = "No active membership", isActive = false });
        }

        var plan = _context.MembershipPlans.FirstOrDefault(p => p.MembershipPlanId == membership.MembershipPlanId);

        var isActive = membership.Status == (int)UserMembershipStatus.Active && 
                       membership.EndDate > DateTime.Now;

        var result = new UserMembershipResponseDto
        {
            UserMembershipId = membership.UserMembershipId,
            UserId = membership.UserId,
            StartDate = membership.StartDate,
            EndDate = membership.EndDate,
            IsActive = isActive,
            StatusText = Enum.GetName(typeof(UserMembershipStatus), membership.Status ?? 1) ?? "Active",
            MembershipPlan = plan != null ? new MembershipPlanDto
            {
                MembershipPlanId = plan.MembershipPlanId,
                Name = plan.Name,
                Price = plan.Price,
                DurationDays = plan.DurationDays,
                Description = plan.Description,
                IsActive = plan.IsActive
            } : new MembershipPlanDto()
        };

        return Ok(result);
    }

    /// <summary>
    /// [Customer] Đăng ký một gói membership mới
    /// </summary>
    [HttpPost("subscribe")]
    public async Task<IActionResult> SubscribeToMembership([FromBody] CreateUserMembershipDto request)
    {
        var userId = long.Parse(_httpContextAccessor.HttpContext?.User.FindFirst("UserId")?.Value ?? "0");

        var plan = _context.MembershipPlans.FirstOrDefault(p => p.MembershipPlanId == request.MembershipPlanId);
        if (plan == null)
        {
            return BadRequest("Membership plan not found");
        }

        var existingMembership = _context.UserMemberships
            .FirstOrDefault(m => m.UserId == userId && m.Status == (int)UserMembershipStatus.Active);

        if (existingMembership != null && existingMembership.EndDate > DateTime.Now)
        {
            return BadRequest("User already has an active membership");
        }

        var startDate = DateTime.Now;
        var endDate = startDate.AddDays(plan.DurationDays);

        var membership = new UserMembership
        {
            UserId = userId,
            MembershipPlanId = request.MembershipPlanId,
            PaymentId = request.PaymentId,
            StartDate = startDate,
            EndDate = endDate,
            PricePaid = plan.Price,
            Status = (int)UserMembershipStatus.Active,
            CreatedAt = DateTime.Now
        };

        _context.UserMemberships.Add(membership);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Subscription successful", userMembershipId = membership.UserMembershipId });
    }

    /// <summary>
    /// [Customer] Hủy gói membership hiện tại
    /// </summary>
    [HttpPost("cancel-membership")]
    public async Task<IActionResult> CancelMembership()
    {
        var userId = long.Parse(_httpContextAccessor.HttpContext?.User.FindFirst("UserId")?.Value ?? "0");

        var membership = _context.UserMemberships
            .Where(m => m.UserId == userId && m.Status == (int)UserMembershipStatus.Active)
            .FirstOrDefault();

        if (membership == null)
        {
            return BadRequest("No active membership to cancel");
        }

        membership.Status = (int)UserMembershipStatus.Cancelled;
        _context.UserMemberships.Update(membership);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Membership cancelled successfully" });
    }
}
