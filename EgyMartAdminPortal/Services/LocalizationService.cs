using Microsoft.JSInterop;

namespace EgyMartAdminPortal.Services
{
    public class LocalizationService
    {
        private readonly IJSRuntime _jsRuntime;
        private string _currentLanguage = "en";
        private const string StorageKey = "portal_language";

        public event Action? OnLanguageChanged;

        public string CurrentLanguage => _currentLanguage;
        public bool IsRtl => _currentLanguage == "ar";

        public LocalizationService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task InitializeAsync()
        {
            try
            {
                var stored = await _jsRuntime.InvokeAsync<string>("localStorageHelper.getItem", StorageKey);
                if (!string.IsNullOrWhiteSpace(stored) && (stored == "ar" || stored == "en"))
                {
                    _currentLanguage = stored;
                }
                else
                {
                    _currentLanguage = "en";
                }
            }
            catch
            {
                _currentLanguage = "en";
            }

            await ApplyLanguageToDomAsync();
            OnLanguageChanged?.Invoke();
        }

        public async Task SetLanguageAsync(string lang)
        {
            if (lang != "en" && lang != "ar")
                return;

            if (_currentLanguage == lang)
                return;

            _currentLanguage = lang;

            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorageHelper.setItem", StorageKey, lang);
            }
            catch { }

            await ApplyLanguageToDomAsync();
            OnLanguageChanged?.Invoke();
        }

        private async Task ApplyLanguageToDomAsync()
        {
            try
            {
                var dir = IsRtl ? "rtl" : "ltr";
                await _jsRuntime.InvokeVoidAsync("localizationHelper.setLanguage", _currentLanguage, dir);
            }
            catch { }
        }

        public string this[string key] => Translate(key);

        public string T(string key, string? defaultText = null) => Translate(key, defaultText);

        public string Translate(string key, string? defaultText = null)
        {
            if (string.IsNullOrEmpty(key))
                return string.Empty;

            if (_currentLanguage != "ar")
                return defaultText ?? key;

            var trimmed = key.Trim();
            if (ArabicDictionary.TryGetValue(trimmed, out var arabicText))
                return arabicText;

            return defaultText ?? key;
        }

        private static readonly Dictionary<string, string> ArabicDictionary = new(StringComparer.OrdinalIgnoreCase)
        {
            // Navigation
            { "Dashboard", "لوحة التحكم" },
            { "Contact Us Page", "صفحة اتصل بنا" },
            { "User Managment", "إدارة المستخدمين" },
            { "User Management", "إدارة المستخدمين" },
            { "Pending Account", "الحسابات المعلقة" },
            { "Supplier", "الموردين" },
            { "Customer", "العملاء" },
            { "Attributes", "الخصائص والمواصفات" },
            { "Product Categories", "فئات المنتجات" },
            { "Stores Management", "إدارة المتاجر" },
            { "Coupon Management", "إدارة الكوبونات" },
            { "Ads Subscriptions", "اشتراكات الإعلانات" },
            { "News List Subscriptions", "اشتراكات القائمة البريدية" },
            { "CMS", "إدارة المحتوى" },
            { "Social Media", "وسائل التواصل الاجتماعي" },
            { "Header Menu", "القائمة العلوية" },
            { "Slider", "شرائح العرض (السلايدر)" },
            { "Slider Menu", "شرائح العرض" },
            { "Footer", "تذييل الصفحة" },
            { "Pages", "الصفحات الثابتة" },
            { "FAQs", "الأسئلة الشائعة" },
            { "Admin Portal", "بوابة الإدارة" },

            // General Actions & Controls
            { "Add", "إضافة" },
            { "Create", "إنشاء" },
            { "Edit", "تعديل" },
            { "Delete", "حذف" },
            { "Verify", "توثيق" },
            { "Verified", "موثق" },
            { "Unverified", "غير موثق" },
            { "Save", "حفظ" },
            { "Saving...", "جاري الحفظ..." },
            { "Cancel", "إلغاء" },
            { "Close", "إغلاق" },
            { "Confirm", "تأكيد" },
            { "Search", "بحث" },
            { "Search...", "بحث..." },
            { "Filter", "تصفية" },
            { "All", "الكل" },
            { "Status", "الحالة" },
            { "Active", "نشط" },
            { "Inactive", "غير نشط" },
            { "Actions", "الإجراءات" },
            { "Refresh", "تحديث" },
            { "Export", "تصدير" },
            { "Export to Excel", "تصدير إلى إكسيل" },
            { "Translate", "ترجمة" },
            { "Back", "رجوع" },
            { "Yes", "نعم" },
            { "No", "لا" },
            { "Loading...", "جاري التحميل..." },
            { "No data found", "لا توجد بيانات متاحة" },
            { "No data available", "لا توجد بيانات متاحة" },
            { "Details", "التفاصيل" },
            { "Success", "نجاح" },
            { "Error", "خطأ" },
            { "Warning", "تحذير" },
            { "Info", "معلومات" },
            { "Required", "مطلوب" },
            { "Submit", "إرسال" },
            { "Reset", "إعادة ضبط" },
            { "Clear", "مسح" },
            { "Previous", "السابق" },
            { "Next", "التالي" },
            { "Page", "صفحة" },
            { "of", "من" },
            { "Total", "الإجمالي" },
            { "Total Items", "إجمالي العناصر" },
            { "Items per page", "عنصر لكل صفحة" },
            { "Showing", "عرض" },
            { "items", "عناصر" },
            { "ID", "المعرف" },
            { "Name", "الاسم" },
            { "Email", "البريد الإلكتروني" },
            { "Phone", "رقم الهاتف" },
            { "Mobile", "المحمول" },
            { "Address", "العنوان" },
            { "Description", "الوصف" },
            { "Created Date", "تاريخ الإنشاء" },
            { "Created At", "تاريخ الإنشاء" },
            { "Updated At", "تاريخ التحديث" },
            { "Image", "الصورة" },
            { "Logo", "الشعار" },
            { "Select", "اختر" },
            { "Upload", "رفع" },
            { "Download", "تحميل" },
            { "View", "عرض" },
            { "Approve", "موافقة" },
            { "Reject", "رفض" },
            { "Deny", "رفض" },
            { "Pending", "قيد الانتظار" },
            { "Denied", "مرفوض" },
            { "Are you sure?", "هل أنت متأكد؟" },

            // Stores Management
            { "Add New Store", "إضافة متجر جديد" },
            { "Create New Store", "إنشاء متجر جديد" },
            { "Edit Store", "تعديل المتجر" },
            { "Store Details", "تفاصيل المتجر" },
            { "Verify Store", "توثيق المتجر" },
            { "Delete Store", "حذف المتجر" },
            { "Store Name", "اسم المتجر" },
            { "Store Description", "وصف المتجر" },
            { "Store Details & Settings", "تفاصيل وإعدادات المتجر" },
            { "Physical Pickup Stores & Vendor Locations", "المتاجر المادية ومواقع استلام البائعين" },
            { "Vendor / Owner", "البائع / المالك" },
            { "Vendor", "البائع" },
            { "Vendor ID", "رقم البائع" },
            { "Location", "الموقع" },
            { "City", "المدينة" },
            { "State", "المحافظة" },
            { "Country", "الدولة" },
            { "Postal Code", "الرمز البريدي" },
            { "Commercial Register", "السجل التجاري" },
            { "Tax Card", "البطاقة الضريبية" },
            { "Verification", "التوثيق" },
            { "Verification Status", "حالة التوثيق" },
            { "Verified Products", "المنتجات الموثقة" },
            { "Store Products", "منتجات المتجر" },
            { "Products Verification", "توثيق المنتجات" },
            { "Manage Products", "إدارة المنتجات" },
            { "No Stores Found", "لم يتم العثور على متاجر" },
            { "Create First Store", "إنشاء أول متجر" },
            { "Search by store name, address, city, or vendor...", "البحث باسم المتجر، العنوان، المدينة، أو البائع..." },
            { "All Verification Statuses", "جميع حالات التوثيق" },
            { "Verified Only", "الموثقة فقط" },
            { "Pending Verification Only", "بانتظار التوثيق فقط" },
            { "Store Admin Controls", "أدوات إدارة المتجر" },
            { "Store Information", "معلومات المتجر" },
            { "Store Logo & Verification", "شعار المتجر والتوثيق" },
            { "Location on Map", "الموقع على الخريطة" },
            { "Latitude", "خط العرض" },
            { "Longitude", "خط الطول" },
            { "Verify Product", "توثيق المنتج" },
            { "Reject Product", "رفض المنتج" },
            { "Product Name", "اسم المنتج" },
            { "SKU", "رمز المنتج (SKU)" },
            { "Price", "السعر" },
            { "Stock", "المخزون" },
            { "Are you sure you want to delete this store?", "هل أنت متأكد من رغبتك في حذف هذا المتجر؟" },
            { "Store created successfully", "تم إنشاء المتجر بنجاح" },
            { "Store updated successfully", "تم تحديث المتجر بنجاح" },
            { "Store deleted successfully", "تم حذف المتجر بنجاح" },
            { "Store verified successfully", "تم توثيق المتجر بنجاح" },
            { "Store rejected successfully", "تم رفض توثيق المتجر" },
            { "Product verified successfully", "تم توثيق المنتج بنجاح" },
            { "Product rejected successfully", "تم رفض المنتج بنجاح" },
            { "Manage physical pickup locations, verify vendor stores, and approve store products.", "إدارة مواقع الاستلام، توثيق متاجر البائعين، والموافقة على منتجات المتاجر." },
            { "No pickup stores have been registered yet.", "لم يتم تسجيل أي متاجر استلام حتى الآن." },
            { "No stores match your search criteria.", "لا توجد متاجر مطابقة لمعايير البحث." },

            // Coupon Management
            { "Add New Coupon", "إضافة كوبون جديد" },
            { "Create New Coupon", "إنشاء كوبون جديد" },
            { "Edit Coupon", "تعديل الكوبون" },
            { "Coupon Details", "تفاصيل الكوبون" },
            { "Delete Coupon", "حذف الكوبون" },
            { "Coupon Code", "كود الكوبون" },
            { "Discount Amount", "قيمة الخصم" },
            { "Discount Type", "نوع الخصم" },
            { "Percentage (%)", "نسبة مئوية (%)" },
            { "Fixed Amount ($)", "مبلغ ثابت ($)" },
            { "Minimum Spend", "الحد الأدنى للإنفاق" },
            { "Max Uses", "الحد الأقصى للاستخدام" },
            { "Uses Count", "عدد مرات الاستخدام" },
            { "Valid From", "صالح من" },
            { "Valid To", "صالح حتى" },
            { "Scope / Store", "النطاق / المتجر" },
            { "Scope", "النطاق" },
            { "All Stores", "جميع المتاجر" },
            { "All Stores (Platform-Wide)", "جميع المتاجر (على مستوى المنصة)" },
            { "Store Specific", "متجر محدد" },
            { "Store-Specific (Vendor)", "متجر محدد (تابع لبائع)" },
            { "Coupon Type", "نوع الكوبون" },
            { "Select Store", "اختر المتجر" },
            { "Select a store...", "اختر متجراً..." },
            { "Select discount type...", "اختر نوع الخصم..." },
            { "Total Coupons", "إجمالي الكوبونات" },
            { "Active Coupons", "الكوبونات النشطة" },
            { "Total Redemptions", "إجمالي مرات الاستخدام" },
            { "Expired / Inactive", "منتهية الصلاحية / غير نشطة" },
            { "No Coupons Found", "لم يتم العثور على كوبونات" },
            { "Create First Coupon", "إنشاء أول كوبون" },
            { "No coupons have been created yet.", "لم يتم إنشاء أي كوبونات بعد." },
            { "Search by coupon code or store name...", "البحث بكود الكوبون أو اسم المتجر..." },
            { "All Discount Types", "جميع أنواع الخصم" },
            { "Percentage Only", "نسبة مئوية فقط" },
            { "Fixed Amount Only", "مبلغ ثابت فقط" },
            { "All Scopes", "جميع النطاقات" },
            { "Platform-Wide Only", "على مستوى المنصة فقط" },
            { "Store-Specific Only", "متاجر محددة فقط" },
            { "All Statuses", "جميع الحالات" },
            { "Active Only", "النشطة فقط" },
            { "Expired Only", "المنتهية فقط" },
            { "No coupons match your search criteria.", "لا توجد كوبونات مطابقة لمعايير البحث." },
            { "Coupon Details & Rules", "تفاصيل وقواعد الكوبون" },
            { "Scope & Applicability", "النطاق وقابلية التطبيق" },
            { "Are you sure you want to delete coupon", "هل أنت متأكد من رغبتك في حذف الكوبون" },
            { "This action cannot be undone.", "لا يمكن التراجع عن هذا الإجراء." },
            { "Coupon created successfully", "تم إنشاء الكوبون بنجاح" },
            { "Coupon deleted successfully", "تم حذف الكوبون بنجاح" },
            { "Coupon status updated", "تم تحديث حالة الكوبون" },
            { "Expired", "منتهي الصلاحية" },
            { "Exhausted", "استُنفذ بالكامل" },
            { "Uses", "الاستخدامات" },
            { "Discount", "الخصم" },
            { "Create discount codes, configure spending rules and usage limits, and manage vendor-scoped or platform-wide coupons.", "إنشاء أكواد الخصم، وتحديد شروط الإنفاق وحدود الاستخدام، وإدارة كوبونات المتاجر أو المنصة العامة." },

            // User Management & Verification
            { "Users", "المستخدمين" },
            { "First Name", "الاسم الأول" },
            { "Last Name", "اسم العائلة" },
            { "User Name", "اسم المستخدم" },
            { "Role", "الصلاحية / الدور" },
            { "National ID", "الرقم القومي" },
            { "Documents", "المستندات" },
            { "Show Attachment", "عرض المرفق" },
            { "User Verified Successfully", "تم توثيق المستخدم بنجاح" },
            { "User Denied Successfully", "تم رفض المستخدم بنجاح" },

            // Header & Profile
            { "Profile", "الملف الشخصي" },
            { "Logout", "تسجيل الخروج" },
            { "Change Password", "تغيير كلمة المرور" },
            { "New Password", "كلمة المرور الجديدة" },
            { "Confirm Password", "تأكيد كلمة المرور" },
            { "Enter Your Password", "أدخل كلمة المرور" },
            { "Change", "تغيير" },
            { "Language", "اللغة" },
            { "English", "English" },
            { "Arabic", "العربية" },

            // Categories & Attributes
            { "Category Name", "اسم الفئة" },
            { "Parent Category", "الفئة الرئيسية" },
            { "Attribute Name", "اسم الخاصية" },
            { "Attribute Values", "قيم الخاصية" },
            { "Add Category", "إضافة فئة" },
            { "Add Attribute", "إضافة خاصية" },

            // Dashboard
            { "Total Sales", "إجمالي المبيعات" },
            { "Total Orders", "إجمالي الطلبات" },
            { "Total Products", "إجمالي المنتجات" },
            { "Total Stores", "إجمالي المتاجر" },
            { "Total Users", "إجمالي المستخدمين" },
            { "Recent Orders", "أحدث الطلبات" },
            { "Top Selling Products", "المنتجات الأكثر مبيعاً" },
            { "Trending Stores", "المتاجر الأكثر تفاعلاً" },
            { "Sales Overview", "نظرة عامة على المبيعات" },

            // Newsletter & Notifications
            { "Newsletter & Notifications", "النشرة البريدية والإشعارات" },
            { "Newsletter", "النشرة البريدية" },
            { "Notifications", "الإشعارات" },
            { "Broadcast Notification", "إرسال إشعار عام" },
            { "Send Broadcast", "إرسال الإشعار" },
            { "Send Notification", "إرسال إشعار" },
            { "Subscribers", "المشتركون" },
            { "Broadcast History", "سجل الإرسال" },
            { "SignalR Monitor", "مراقبة الاتصال المباشر" },
            { "Target Audience", "الجمهور المستهدف" },
            { "All Recipients", "الكل (عملاء وموردين)" },
            { "Clients", "العملاء فقط" },
            { "Vendors", "الموردون فقط" },
            { "Notification Title", "عنوان الإشعار" },
            { "Notification Message", "نص الإشعار" },
            { "Attachment (Image, PDF, File)", "مرفق (صورة، PDF، ملف)" },
            { "Choose File", "اختر ملفاً" },
            { "No File Chosen", "لم يتم اختيار ملف" },
            { "Sent Date", "تاريخ الإرسال" },
            { "Recipients", "المستلمون" },
            { "Sent Successfully", "تم الإرسال بنجاح" },
            { "Failed to Send", "فشل الإرسال" },
            { "Subscribed At", "تاريخ الاشتراك" },
            { "Unsubscribed At", "تاريخ إلغاء الاشتراك" },
            { "Unsubscribe", "إلغاء الاشتراك" },
            { "Reactivate", "إعادة تفعيل" },
            { "Delete Subscriber", "حذف المشترك" },
            { "Add Subscriber", "إضافة مشترك" },
            { "Email Address", "البريد الإلكتروني" },
            { "User Type", "نوع المستخدم" },
            { "Connected", "متصل" },
            { "Disconnected", "غير متصل" },
            { "Real-Time SignalR Status", "حالة الاتصال المباشر SignalR" },
            { "Total Broadcasts", "إجمالي الإشعارات المرسلة" },
            { "Total Subscribers", "إجمالي المشتركين" },
            { "Active Subscribers", "المشتركون النشطون" },
            { "Unsubscribed", "ملغى الاشتراك" },
            { "Download / View", "عرض / تحميل" },
            { "Live Notification Feed", "تغذية الإشعارات اللحظية" },
            { "No Notifications Received Yet", "لم يتم استلام إشعارات بعد في هذه الجلسة" }
        };
    }
}
