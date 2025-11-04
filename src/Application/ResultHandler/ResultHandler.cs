

namespace Application.ResultHandler
{
    public class ResultHandler<ReturnedData>
    {
        public bool IsSuccess { get; }
        public ReturnedData Response { get; }
        public string ErrorMessage { get; }
        public object ErrorResponse { get; }

        private ResultHandler(ReturnedData someData, bool isSucces, string errorMess,object errorBody=null)
        {
            Response = someData;
            IsSuccess = isSucces;
            ErrorMessage = errorMess;
            if (errorBody != null)
            {
                ErrorResponse = errorBody;
            }
        }

        public static ResultHandler<ReturnedData> Success(ReturnedData data)
        {
            ResultHandler<ReturnedData> succ = new ResultHandler<ReturnedData>(data, true, null);
            return succ;
        }
        public static ResultHandler<ReturnedData> Fail(string erorDescription,object errorBody=null)
        {
            ResultHandler<ReturnedData> fail = new ResultHandler<ReturnedData>(default, false, erorDescription,errorBody);
            return fail;
        }

    }
}
