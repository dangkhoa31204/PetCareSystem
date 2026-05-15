namespace PetCareSystem.API.Dtos.Common
{
    public class BaseResponseDto<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
}
