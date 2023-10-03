namespace GameServer.Utilities
{
    public class Constants
    {
        public static string MAIL_KEY = "b2bca895371900c9067a08621240a008";
        public static string GO_URL = "http://www.go.vn/";

        public static string SERVER_VTC_ADDR = "http://localhost:8084/";//"http://sandbox.vtcgame.vn/";

        public static string SSO_URL_CONTINUE = "http://localhost:5706/login/default.aspx";

        public static string SERVICE_PROVIDER_SITE_ID = "100000";

        //Sai thông số đầu vào
        public const int EXCEPTION_CODE = -69;
        public const string EXCEPTION_MESSAGE = "Thông số đầu vào không đúng. Hoặc hệ thống bận!";

        //Đăng nhập không thành công
        public const int LOGIN_ERROR_CODE = -2501;
        public const string LOGIN_ERROR_MESSAGE = "Đăng nhập không thành công , mời thử lại";

        //Hết phiên đăng nhập
        public const int SESSION_EXPRIED_CODE = -2502;
        public const string SESSION_EXPRIED_MESSAGE = "Phiên đăng nhập của bạn đã hết. Xin vui lòng đăng nhập lại!";

        //Sai thông số đầu vào
        public const int INPUT_ERROR_CODE = -2503;
        public const string INPUT_ERROR_MESSAGE = "Thông số đầu vào không đúng. Xin vui lòng thử lại!";

        //Sai thông số đầu vào
        public const int NOTLOGIN_ERROR_CODE = -2504;
        public const string NOTLOGIN_ERROR_MESSAGE = "Bạn chưa đăng nhập.";

        //Authen nhiều lần
        public const int LIMIT_LOGIN_ERROR_CODE = -2505;
        public const string LIMIT_LOGIN_ERROR_MESSAGE = "Tài khoản và IP đã Đăng nhập quá nhiều!Vui lòng chờ!";

        //Thao tác nhiều lần
        public const int LIMIT_ACTION_ERROR_CODE = -2506;
        public const string LIMIT_ACTION_ERROR_MESSAGE = "Bạn đã thao tác nhanh vào nhiều lần. Vui lòng chờ !";

        //Thao tác nhiều lần
        public const int TOKEN_PARSE_ERROR_CODE = -2507;
        public const string TOKEN_PARSE_ERROR_MESSAGE = "Token truyền lên không hợp lệ. Mời kiểm tra lại!";
    }
}