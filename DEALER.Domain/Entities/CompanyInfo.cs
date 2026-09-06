namespace DEALER.Domain
{
    public class CompanyInfo
    {
        public int Id {  get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string WhatsApp { get; set; }
        public string WebSiteLink { get; set; }
        public string Email { get; set; }
        public string Email2 { get; set; }
        public string DhakaOfficeAddress { get; set; }
        public string HeadOfficeAddress { get; set; }
        public string SpecialNotice { get; set; }
        public string FacebookPage { get; set; }
        public string FacebookPage2 { get; set; }
        public string FacebookPage3 { get; set; }
        public string Logo { get; set; }
        public byte[]? Image { get; set; }
        public string Bkash { get; set; }
        public string Nagad { get; set; }
        public string Rocket { get; set; }
        public string CompanyMessage { get; set; }
    }


    public class LicenseInfo
    {
        public int Id { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Message { get; set; } = "আপনার সফটওয়্যারের মেয়াদ শেষ হয়ে গেছে। অনুগ্রহ করে পেমেন্ট করে যোগাযোগ করুন।";
    }
}