using EgyMartAdminPortal.Models;
using EgyMartAdminPortal.Models.Dashoard;
using System.Net.Http.Json;

namespace EgyMartAdminPortal.Services
{
    public class DashboardService(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;
        protected string ApiUrl = "reporting/api/v2/Reporting/AdminDashboard";
        //01
        public async Task<List<AgeData>> ViewCountByAge()
        {
            var response = await _httpClient.GetFromJsonAsync<ApiAgeResponse>($"{ApiUrl}/ViewCountByAge");

            return response?.Data ?? [];
        }
        //02
        public async Task<List<UserSub>> UserSubscriptionWillFinish(int WillFinishDays)
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<UserSub>>>($"{ApiUrl}/UsersSubscriptionWillFinish?WillFinishDays={WillFinishDays}"))!;
            return response.Data;
            //return [
            //    new() { PlanID = 1, UserID = 1001, DisplayName = "User A", EndDate = DateTime.Now.AddDays(30) },
            //    new() { PlanID = 2, UserID = 1002, DisplayName = "User B", EndDate = DateTime.Now.AddDays(10) },
            //    new() { PlanID = 3, UserID = 1003, DisplayName = "User C", EndDate = DateTime.Now.AddDays(15) },
            //    new() { PlanID = 4, UserID = 1004, DisplayName = "User D", EndDate = DateTime.Now.AddDays(20) },
            //    new() { PlanID = 5, UserID = 1005, DisplayName = "User E", EndDate = DateTime.Now.AddDays(25) },
            //    new() { PlanID = 6, UserID = 1006, DisplayName = "User F", EndDate = DateTime.Now.AddDays(25) },
            //    new() { PlanID = 3, UserID = 1007, DisplayName = "User G", EndDate = DateTime.Now.AddDays(13) },
            //    new() { PlanID = 4, UserID = 1008, DisplayName = "User H", EndDate = DateTime.Now.AddDays(20) },
            //    new() { PlanID = 2, UserID = 1009, DisplayName = "User I", EndDate = DateTime.Now.AddDays(12) },
            //    new() { PlanID = 5, UserID = 1010, DisplayName = "User J", EndDate = DateTime.Now.AddDays(15) },
            //    new() { PlanID = 1, UserID = 1011, DisplayName = "User B", EndDate = DateTime.Now.AddDays(5) },
            //    new() { PlanID = 1, UserID = 1012, DisplayName = "User C", EndDate = DateTime.Now.AddDays(3) },
            //    new() { PlanID = 1, UserID = 1013, DisplayName = "User D", EndDate = DateTime.Now.AddDays(10) },
            //    new() { PlanID = 1, UserID = 1014, DisplayName = "User E", EndDate = DateTime.Now.AddDays(7) },
            //    new() { PlanID = 1, UserID = 1015, DisplayName = "User F", EndDate = DateTime.Now.AddDays(25) },
            //    new() { PlanID = 1, UserID = 1016, DisplayName = "User G", EndDate = DateTime.Now.AddDays(13) },
            //    new() { PlanID = 1, UserID = 1017, DisplayName = "User H", EndDate = DateTime.Now.AddDays(20) },
            //    new() { PlanID = 1, UserID = 1018, DisplayName = "User I", EndDate = DateTime.Now.AddDays(12) },
            //    new() { PlanID = 1, UserID = 1019, DisplayName = "User J", EndDate = DateTime.Now.AddDays(15) },
            //    new() { PlanID = 1, UserID = 1020, DisplayName = "User J", EndDate = DateTime.Now.AddDays(15) },
            //];
        }
        //03
        public async Task<List<PlanData>> UserSubscriptionCountByPlan()
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<PlanData>>>($"{ApiUrl}/UserSubscriptionCountByPlan"))!;
            return response.Data;
            //return [
            //    new() { PlanId = 1, PlanName="Free Ads for 30 Days", PlansCount = 1000 },
            //    new() { PlanId = 2, PlanName="Store Front Pop-up Again", PlansCount = 750 },
            //    new() { PlanId = 3, PlanName="Store Front Location 1", PlansCount = 3000 },
            //    new() { PlanId = 4, PlanName="Store Front Location 2", PlansCount = 500 },
            //    new() { PlanId = 5, PlanName="Store Front Location 3", PlansCount = 250 },
            //    new() { PlanId = 6, PlanName="Store Front Location 4", PlansCount = 1200 }
            //    ];
        }
        //04
        public async Task<List<SubscriptionData>> TrendingUserSubscriptionsCount(int topSelect)
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<SubscriptionData>>>($"{ApiUrl}/TrendingUserSubscriptionsCount?topSelect={topSelect}"))!;
            return response.Data;
        }
        //05
        public async Task<List<ProductData>> TrendingProductHistoryGetTop10(int lastXMonths)
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<ProductData>>>($"{ApiUrl}/TrendingProductHistoryGetTop10?lastXMonths={lastXMonths}"))!;
            return response.Data;
        }
        //06
        public async Task<List<CategoryData>> TrendingCategory(int days, int top)
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<CategoryData>>>($"{ApiUrl}/TrendingCategory?days={days}&top={top}"))!;
            return response.Data ?? [];
        }
        //07
        public async Task<List<ViewDayData>> TotalViewsByWeekdays()
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<ViewDayData>>>($"{ApiUrl}/TotalViewsByWeekdays"))!;
            return response.Data ?? [];
        }
        //08
        public async Task<List<RegisterData>> TotaViewByRegister(int days)
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<RegisterData>>>($"{ApiUrl}/TotalViewByRegister?days={days}"))!;
            return response.Data;
        }
        //09
        public async Task<List<GenderData>> TotalProductViewByGender()
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<GenderData>>>($"{ApiUrl}/TotalProductViewByGender"))!;
            return response.Data;
        }
        //10
        public async Task<List<CostPlanData>> TotalCostByPlan()
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<CostPlanData>>>($"{ApiUrl}/TotalCostByPlan"))!;
            return response.Data;
            //return [
            //    new() { PlanID = 1, TotalCost = 100000 },
            //    new() { PlanID = 2, TotalCost = 75000 },
            //    new() { PlanID = 3, TotalCost = 300000 },
            //    new() { PlanID = 4, TotalCost = 50000 },
            //    new() { PlanID = 5, TotalCost = 25000 },
            //    new() { PlanID = 6, TotalCost = 1200 }
            //];
        }
        //11
        public async Task<List<UsersData>> TopUsersViewProducts(int days, int top)
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<UsersData>>>($"{ApiUrl}/TopUsersViewProducts?days={days}&top={top}"))!;
            return response.Data;
        }
        //12
        public async Task<List<RateData>> TopRateProducts(int top)
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<RateData>>>($"{ApiUrl}/TopRateProducts?top={top}"))!;
            return response.Data;
        }
        //13
        public async Task<List<ProductViewData>> ProductTotalView(long productID, int days)
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<ProductViewData>>>($"{ApiUrl}/ProductTotalView?productID={productID}&days={days}"))!;
            return response.Data;
        }
        //14
        public async Task<List<ProductDayData>> ProductViewCountEveryDay(int days)
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<ProductDayData>>>($"{ApiUrl}/ProductsViewCountEveryDay?days={days}"))!;
            return response.Data;
        }
        //15
        public async Task<List<PlatformData>> PlatformsViewCount(int days)
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<PlatformData>>>($"{ApiUrl}/PlatformsViewCount?days={days}"))!;
            return response.Data;
        }
        //16
        public async Task<int> SubscriptionCount()
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<int>>($"{ApiUrl}/Pending/SubscriptionCount"))!;
            return response.Data;
        }
        //17
        public async Task<List<UserTypeData>> UsersVerifiedCount()
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<UserTypeData>>>($"{ApiUrl}/Pending/UsersVerifiedCount"))!;
            return response.Data;
        }
        //18
        public async Task<int> ProductsVerifiedCount()
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<int>>($"{ApiUrl}/Pending/ProductsVerifiedCount"))!;
            return response.Data;
        }
        //19
        public async Task<int> AdsVerifiedCount()
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<int>>($"{ApiUrl}/Pending/AdsVerifiedCount"))!;
            return response.Data;
        }
        //20
        public async Task<List<OSData>> OSViewsCount(int days)
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<OSData>>>($"{ApiUrl}/OSViewsCount?days={days}"))!;
            return response.Data;
        }
        //21
        public async Task<List<LocationData>> LocationViewCount(int days)
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<LocationData>>>($"{ApiUrl}/LocationViewCount?days={days}"))!;
            return response.Data;
        }
        //22
        public async Task<List<TotalViewData>> LastMonthTotalViewByRegister(int days)
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<TotalViewData>>>($"{ApiUrl}/LastMonthTotalViewByRegister?lastMonths={days}"))!;
            return response.Data;
        }
        //23
        public async Task<int> LastMonthsUserJoined(int lastMonths)
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<int>>($"{ApiUrl}/LastMonthsUserJoined?lastMonths={lastMonths}"))!;
            return response.Data;
        }
        //24
        public async Task<int> LastDaysUserJoined(int lastDays)
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<int>>($"{ApiUrl}/LastDaysUserJoined?lastDays={lastDays}"))!;
            return response.Data;
        }
        //25
        public async Task<List<IPAddressData>> IpAdressViewCount(int lastDays)
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<IPAddressData>>>($"{ApiUrl}/IpAdressViewCount?lastDays={lastDays}"))!;
            return response.Data;
        }
        //26
        public async Task<List<CategoryViewData>> CategoryViewSumByDays(int lastDays)
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<CategoryViewData>>>($"{ApiUrl}/CategoryViewSumByDays?lastDays={lastDays}"))!;
            return response.Data;
        }
        //27
        public async Task<List<BrowserData>> BrowserViewsCount(int lastDays)
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<BrowserData>>>($"{ApiUrl}/BrowserViewsCount?lastDays={lastDays}"))!;
            return response.Data;
        }
        //28
        public async Task<List<ProductViewData>> AllProductsViewCount(int lastDays)
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<ProductViewData>>>($"{ApiUrl}/AllProductsViewCount?lastDays={lastDays}"))!;
            return response.Data;
        }
    }
}
