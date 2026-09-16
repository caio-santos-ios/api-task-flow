using to_do_list.src.Interfaces;
using to_do_list.src.Repository;
using to_do_list.src.Services;
using to_do_list.src.Infraestructure;
using to_do_list.src.Helpers;
using to_do_list.src.Interfaces.INotification;
using to_do_list.src.Repositories;
using to_do_list.src.Works;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using CloudinaryDotNet;

namespace to_do_list.src.Configuration
{
    public static class Build
    {
        public static void AddBuilderConfiguration(this WebApplicationBuilder builder)
        {
            AppDbContext.ConnectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? "";
            AppDbContext.DatabaseName = Environment.GetEnvironmentVariable("DATABASE_NAME") ?? "";
            AppDbContext.IsSSL = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("IS_SSL"))
                && Convert.ToBoolean(Environment.GetEnvironmentVariable("IS_SSL"));
        }

        public static void AddFirebase(this WebApplicationBuilder builder)
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                string credentialsPath = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS_PATH") ?? "";

                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile(credentialsPath)
                });
            }
        }

        public static void AddContext(this WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<AppDbContext>();
        }

        public static void AddBuilderHelpers(this WebApplicationBuilder builder)
        {
            builder.Services.AddHttpClient<MailHelper>();
            builder.Services.AddTransient<UploadHelper>();
        }

        public static void AddBuilderServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddTransient<IAuthService, AuthService>();
            builder.Services.AddTransient<IUserService, UserService>();
            builder.Services.AddTransient<IUserRepository, UserRepository>();
            builder.Services.AddTransient<ICategoryService, CategoryService>();
            builder.Services.AddTransient<ICategoryRepository, CategoryRepository>();
            builder.Services.AddTransient<ITaskService, TaskService>();
            builder.Services.AddTransient<ITaskRepository, TaskRepository>();
            builder.Services.AddTransient<IDashboardService, DashboardService>();
            builder.Services.AddTransient<IDashboardRepository, DashboardRepository>();
            builder.Services.AddTransient<INotificationService, NotificationService>();
            builder.Services.AddTransient<INotificationRepository, NotificationRepository>();

            builder.Services.AddHostedService<PushNotificationWork>();

            builder.Services.AddAutoMapper(cfg => { }, typeof(Program));
            
            // CLOUDINARY
            Account account = new(
                Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME"),
                Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY"),
                Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET")
            );
            builder.Services.AddSingleton(new Cloudinary(account));
        }
    }
}