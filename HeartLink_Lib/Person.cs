using System;

namespace HeartLink_Lib
{
    public class Person : IComparable
    {

        public Person()
        { }

        public Person Clone()
        {
            Person temp = new Person();
            temp.ID = ID;
            temp.NAME = NAME;
            temp.CELLPHONENUMBER = CELLPHONENUMBER;
            temp.GENDER = GENDER;
            temp.PASSWORD = PASSWORD;
            return temp;
        }

        #region Properties
        public string ID { get; set; }

        public string NAME { get; set; }

        public string CELLPHONENUMBER { get; set; }

        public string GENDER { get; set; }

        public string PASSWORD { get; set; }
        #endregion

        public int CompareTo(Object obj)
        {
            if (obj is Person)
                return (int)(Convert.ToInt64(ID) - Convert.ToInt64((obj as Person).ID));
            else
                throw new ArgumentException("Object must be a Person.");
        }
    }
}
