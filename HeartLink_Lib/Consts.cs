namespace HeartLink_Lib
{
    public enum CommandCode
    {
        //通用命令0x00000000开始
        Exit,                               //客户端关闭app，服务器线程断开socket       
        Succeed,
        Error,

        Login,
        Logout,
        WrongPsw,
        NotRegistered,
        AlreadyLogin,

        Register,
        AlreadyRegistered,
        RegisterFailed,

        SendContacter,
        GetContacters,
        ReturnContacters,

        SendMessageBox,
        GetMessageBox,
        ReturnMessageBox,

        SendDailyReminder,
        GetDailyReminder,
        ReturnDailyReminder,

        SendLocation,
        GetLocation,
        ReturnLocation,

        SendAskOff,
        GetAskOff,
        ReturnAskOff,
        SolveAskOff,

        SendDelivery,
        GetDelivery,
        ReturnDelivery,
        SolveDelivery,

        SendRepair,
        GetRepair,
        ReturnRepair,
        SolveRepair,

        SendWater,
        GetWater,
        ReturnWater,
        SolveWater,
    }

    public static class Consts
    {
        public const string Online = "online";
        public const string Offline = "offline";
        public const string Male = "Male";
        public const string Female = "Female";

        public const int IDLength = 10;
        public const int NameLength = 20;
        public const int GenderLength = 6;
        public const int CellphoneNumberLength = 11;
        public const int AddressLength = 6;
        public const int ClassNumberLength = 10;
        public const int PasswordLength_MAX = 15;
        public const int PasswordLength_MIN = 4;
        public const int DormitoryBuildingLength = 2;
        public const int TextContentLength = 100;
    }

}
