namespace Yavsc.Abstract.Helpers
{
    public enum ErrorCode {
        NotFound,
        InternalError,
        DestExists, 
        InvalidRequest
    }

    public class FsOperationInfo {

        public bool Done { get; set; } = false;

        public ErrorCode ErrorCode { get; set; }
        public string ErrorMessage { get; set; }

    }
}
