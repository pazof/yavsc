


namespace BookAStar.Model.Social
{

    public class CompanyInfoMessage
    {
        public bool success { get; set; }
        public string errorType { get; set; }
        public string errorCode { get; set; }
        public string errorMessage { get; set; }
        public CompanyInfo result { get; set; }

    }

    public class CompanyInfo
    {
        public string registration_number { get; set; }
        public string vat_number { get; set; }
        public string name { get; set; }
        public string commercial_name { get; set; }
        public string business_name { get; set; }
        public string ape_code { get; set; }
        public string ape_label { get; set; }
        public string rcs_name { get; set; }
        public string greffe_name { get; set; }
        public string legal_person_type { get; set; }

        public string legal_state { get; set; }

        public string legal_type { get; set; }

        public string legal_status { get; set; }

        public string activity { get; set; }

        public string street { get; set; }
        public string postal_code { get; set; }
        public string country_code { get; set; }
        public decimal lng { get; set; }
        public decimal lat { get; set; }
        public string phone_number { get; set; }
        public string capital { get; set; }


        public Employee[] contacts { get; set; }
        public string[] emails { get; set; }

        public Stat[] stats { get; set; }

        public MarkDetail[] marks_details { get; set; }






        public Bodacc last_bodacc { get; set; }

    }

    public class MarkDetail
    {
        public MarkClass[] classes { get; set; }
        public Mark[] marks { get; set; }
    }
    public class MarkClass
    {
        public string number { get; set; }
        public string description { get; set; }
    }
    public class Mark
    {
        public string name { get; set; }
        public long application_date { get; set; }
    }

    public class Stat
    {
        public string type { get; set; }
        public int year { get; set; }
        public string value { get; set; }
    }

    public class Bodacc
    {
        public string type { get; set; }
        public string bodacc_type { get; set; }
        public long parution_date { get; set; }
        public string number { get; set; }
        public string rcs_name { get; set; }
        public string legal_name { get; set; }
        public string legal_status { get; set; }
        public string capital { get; set; }
        public string administration { get; set; }
        public string address { get; set; }
        public string description { get; set; }

    }

    public class Employee
    {
        public string name { get; set; }
        public string role { get; set; }
    }

}
