namespace HeartLink_Lib
{
    public class Student : Person
    {

        public Student()
        {
            ID = "0000000000";
            NAME = "Default name";
            CELLPHONENUMBER = "00000000000";
            GENDER = "Male";
            PASSWORD = "Default psw";
            CLASSNUMBER = "0000000000";
            DORMITORYADDRESS = "Default dormitory address";
            DIRECTORNAME = "Default director name";
            DIRECTORCELLPHONENUMBER = "00000000000";
        }

        public void TrimFields()
        {
            ID = ID.Trim();
            NAME = NAME.Trim();
            CELLPHONENUMBER = CELLPHONENUMBER.Trim();
            GENDER = GENDER.Trim();
            PASSWORD = PASSWORD.Trim();
            CLASSNUMBER = CLASSNUMBER.Trim();
            DORMITORYADDRESS = DORMITORYADDRESS.Trim();
            DIRECTORNAME = DIRECTORNAME.Trim();
            DIRECTORCELLPHONENUMBER = DIRECTORCELLPHONENUMBER.Trim();
        }

        public new Student Clone()
        {
            Student temp = new Student();
            temp.ID = ID;
            temp.NAME = NAME;
            temp.CELLPHONENUMBER = CELLPHONENUMBER;
            temp.GENDER = GENDER;
            temp.PASSWORD = PASSWORD;
            temp.CLASSNUMBER = CLASSNUMBER;
            temp.DORMITORYADDRESS = DORMITORYADDRESS;
            temp.DIRECTORNAME = DIRECTORNAME;
            temp.DIRECTORCELLPHONENUMBER = DIRECTORCELLPHONENUMBER;
            return temp;
        }

        #region Properties
        public string CLASSNUMBER { get; set; }

        public string DORMITORYADDRESS { get; set; }

        public string DIRECTORNAME { get; set; }

        public string DIRECTORCELLPHONENUMBER { get; set; }
        #endregion

    }
}
