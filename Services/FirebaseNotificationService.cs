using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiTaller.Data;
using MiTaller.Models.Auth;
using System.Text.Json;

namespace MiTaller.Services
{
    public class FirebaseNotificationService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<BaseIdentityUser> _userManager;
        private readonly DataContext _context;
        private static bool _isInitialized = false;
        private static readonly object _lock = new object();

        public FirebaseNotificationService(
            IConfiguration configuration,
            UserManager<BaseIdentityUser> userManager,
            DataContext context)
        {
            _configuration = configuration;
            _userManager = userManager;
            _context = context;
            InitializeFirebase();
        }

        private void InitializeFirebase()
        {
            if (!_isInitialized)
            {
                lock (_lock)
                {
                    if (!_isInitialized)
                    {
                        try
                        {
                            // Inicializar Firebase Admin SDK con las credenciales del cliente
                            var customerFirebaseConfig = _configuration["Firebase:Customer:ServiceAccountPath"];
                            if (!string.IsNullOrEmpty(customerFirebaseConfig) && File.Exists(customerFirebaseConfig))
                            {
                                var customerCredentials = GoogleCredential.FromFile(customerFirebaseConfig);
                                FirebaseApp.Create(new AppOptions()
                                {
                                    Credential = customerCredentials,
                                    ProjectId = _configuration["Firebase:Customer:ProjectId"]
                                }, "CustomerApp");
                            }

                            // Inicializar Firebase Admin SDK con las credenciales del taller
                            var workshopFirebaseConfig = _configuration["Firebase:Workshop:ServiceAccountPath"];
                            if (!string.IsNullOrEmpty(workshopFirebaseConfig) && File.Exists(workshopFirebaseConfig))
                            {
                                var workshopCredentials = GoogleCredential.FromFile(workshopFirebaseConfig);
                                FirebaseApp.Create(new AppOptions()
                                {
                                    Credential = workshopCredentials,
                                    ProjectId = _configuration["Firebase:Workshop:ProjectId"]
                                }, "WorkshopApp");
                            }

                            _isInitialized = true;
                        }
                        catch (Exception ex)
                        {
                            // Log error pero no fallar la inicialización
                            Console.WriteLine($"Error inicializando Firebase: {ex.Message}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Envía notificación push a un cliente
        /// </summary>
        public async Task<bool> SendNotificationToCustomerAsync(
            Guid customerId,
            string title,
            string body,
            string eventType,
            Dictionary<string, string>? data = null)
        {
            try
            {
                var customer = await _userManager.Users
                    .Where(u => u.Id == customerId && u.UserType == UserType.Customer && !u.IsDeleted)
                    .FirstOrDefaultAsync();

                if (customer == null || string.IsNullOrEmpty(customer.DeviceTokens))
                {
                    return false;
                }

                var tokens = customer.DeviceTokens
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .ToList();

                if (!tokens.Any())
                {
                    return false;
                }

                var app = FirebaseApp.GetInstance("CustomerApp");
                var messaging = FirebaseMessaging.GetMessaging(app);

                // Construir diccionario de datos
                var messageData = new Dictionary<string, string>()
                {
                    { "event", eventType }
                };

                // Agregar datos adicionales si se proporcionan
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        messageData[item.Key] = item.Value;
                    }
                }

                var message = new Message()
                {
                    Notification = new Notification()
                    {
                        Title = title,
                        Body = body
                    },
                    Data = messageData,
                    Android = new AndroidConfig()
                    {
                        Priority = Priority.High
                    },
                    Apns = new ApnsConfig()
                    {
                        Aps = new Aps()
                        {
                            Sound = "default",
                            Badge = 1
                        }
                    }
                };

                // Enviar a múltiples dispositivos
                var sendTasks = tokens.Select(token => 
                {
                    var messageWithToken = new Message()
                    {
                        Token = token,
                        Notification = message.Notification,
                        Data = message.Data,
                        Android = message.Android,
                        Apns = message.Apns
                    };
                    return messaging.SendAsync(messageWithToken);
                });
                var results = await Task.WhenAll(sendTasks);

                // Remover tokens inválidos
                var invalidTokens = new List<string>();
                for (int i = 0; i < results.Length; i++)
                {
                    if (string.IsNullOrEmpty(results[i]))
                    {
                        invalidTokens.Add(tokens[i]);
                    }
                }

                if (invalidTokens.Any())
                {
                    var validTokens = tokens.Except(invalidTokens).ToList();
                    customer.DeviceTokens = string.Join(",", validTokens);
                    await _userManager.UpdateAsync(customer);
                }

                return results.Any(r => !string.IsNullOrEmpty(r));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando notificación a cliente: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Envía notificación push a un taller
        /// </summary>
        public async Task<bool> SendNotificationToWorkshopAsync(
            Guid workshopId,
            string title,
            string body,
            string parentModelType,
            Dictionary<string, string>? data = null)
        {
            try
            {
                var workshop = await _userManager.Users
                    .Where(u => u.Id == workshopId && u.UserType == UserType.Workshop && !u.IsDeleted)
                    .FirstOrDefaultAsync();

                if (workshop == null || string.IsNullOrEmpty(workshop.DeviceTokens))
                {
                    return false;
                }

                var tokens = workshop.DeviceTokens
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .ToList();

                if (!tokens.Any())
                {
                    return false;
                }

                var app = FirebaseApp.GetInstance("WorkshopApp");
                var messaging = FirebaseMessaging.GetMessaging(app);

                // Construir diccionario de datos
                var messageData = new Dictionary<string, string>()
                {
                    { "parentModelType", parentModelType }
                };

                // Agregar datos adicionales si se proporcionan
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        messageData[item.Key] = item.Value;
                    }
                }

                var message = new Message()
                {
                    Notification = new Notification()
                    {
                        Title = title,
                        Body = body
                    },
                    Data = messageData,
                    Android = new AndroidConfig()
                    {
                        Priority = Priority.High
                    },
                    Apns = new ApnsConfig()
                    {
                        Aps = new Aps()
                        {
                            Sound = "default",
                            Badge = 1
                        }
                    }
                };

                // Enviar a múltiples dispositivos
                var sendTasks = tokens.Select(token => 
                {
                    var messageWithToken = new Message()
                    {
                        Token = token,
                        Notification = message.Notification,
                        Data = message.Data,
                        Android = message.Android,
                        Apns = message.Apns
                    };
                    return messaging.SendAsync(messageWithToken);
                });
                var results = await Task.WhenAll(sendTasks);

                // Remover tokens inválidos
                var invalidTokens = new List<string>();
                for (int i = 0; i < results.Length; i++)
                {
                    if (string.IsNullOrEmpty(results[i]))
                    {
                        invalidTokens.Add(tokens[i]);
                    }
                }

                if (invalidTokens.Any())
                {
                    var validTokens = tokens.Except(invalidTokens).ToList();
                    workshop.DeviceTokens = string.Join(",", validTokens);
                    await _userManager.UpdateAsync(workshop);
                }

                return results.Any(r => !string.IsNullOrEmpty(r));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando notificación a taller: {ex.Message}");
                return false;
            }
        }
    }
}

