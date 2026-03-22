namespace SkyLabIdP.Application.Dtos.User.Registration
{
    public class BaseUserRegistrationResponse
    {
        public string UserId { get; set; } = "";
        public string Email { get; set; } = "";

        public string UserName { get; set; } = "";

        public string? FirstName { get; set; } = "";
        public string? LastName { get; set; } = "";
        public OperationResult operationResult { get; set; } = new OperationResult(false, "", 400);
    }
}

