namespace HeartLink_Lib
{
    public class Supervisor : Person
    {

        public Supervisor()
        {
            ID = "0000000000";
            NAME = "Default name";
            CELLPHONENUMBER = "0000000000";
            GENDER = "Male";
            PASSWORD = "Default psw";
            DORMITORYBUILDINGS = "00";
        }

        public void TrimFields()
        {
            ID = ID.Trim();
            NAME = NAME.Trim();
            CELLPHONENUMBER = CELLPHONENUMBER.Trim();
            GENDER = GENDER.Trim();
            PASSWORD = PASSWORD.Trim();
            DORMITORYBUILDINGS = DORMITORYBUILDINGS.Trim();
        }

        public new Supervisor Clone()
        {
            Supervisor temp = new Supervisor();
            temp.ID = ID;
            temp.NAME = NAME;
            temp.CELLPHONENUMBER = CELLPHONENUMBER;
            temp.GENDER = GENDER;
            temp.PASSWORD = PASSWORD;
            temp.DORMITORYBUILDINGS = DORMITORYBUILDINGS;
            return temp;
        }

        public string DORMITORYBUILDINGS { get; set; }

    }
}
