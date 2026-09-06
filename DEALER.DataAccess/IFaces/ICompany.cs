namespace DEALER.DataAccess
{
    public interface ICompany
    {
        bool UpdateCompanyInfo(CompanyInfo companyInfo);
        int CreateCompanyInfo(CompanyInfo companyInfo);
        CompanyInfo GetCompanyInfo();


        Task<byte[]> CreateBackupZipAsync();
    }
}