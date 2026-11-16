namespace MiTaller.Services.Localization
{
    /// Texts the API sends to people (emails, push, inbox, PDFs). English follows tool/l10n/GLOSSARY.md in the apps.
    /// Values may contain HTML only where the key ends in "Html"; arguments are formatted with string.Format.
    public static class T
    {
        public static string Get(string lang, string key, params object?[] args)
        {
            if (!Texts.TryGetValue(key, out var pair)) return key;
            var text = lang == Lang.En ? pair.En : pair.Es;
            return args.Length == 0 ? text : string.Format(text, args);
        }

        private static readonly Dictionary<string, (string Es, string En)> Texts = new()
        {
            // Email shell
            ["email.hi"] = ("Hola", "Hi"),
            ["email.thanks"] = ("¡Saludos, gracias por tu descarga!", "Best regards, and thanks for downloading MiTaller!"),
            ["email.codeExpires"] = ("Este código expirará en 15 minutos.", "This code expires in 15 minutes."),
            ["email.tagline"] = ("MiTaller – Conectando movilidad y seguridad", "MiTaller – Connecting mobility and safety"),
            ["email.confidential"] = (
                "<p><strong>AVISO DE CONFIDENCIALIDAD.</strong> Este correo y la información contenida o adjunta al mismo es privada y confidencial y va dirigida exclusivamente a su destinatario. MiTaller informa a quien pueda haber recibido este correo por error que contiene información confidencial cuyo uso, copia, reproducción o distribución está expresamente prohibida. Si no eres el destinatario del mismo y recibes este correo por error, te pedimos pongas en conocimiento al emisor y procedas a la eliminación sin copiarlo, imprimirlo o utilizarlo de ningún modo.</p>\n    <p><strong>CONFIDENTIALITY WARNING.</strong> This message and the information contained in or attached to it are private and confidential and intended exclusively for the addressee. If you are not an intended recipient of this email, please notify the sender, delete it and do not read, act upon, print, disclose, copy, retain or redistribute any portion of it.</p>",
                "<p><strong>CONFIDENTIALITY WARNING.</strong> This message and the information contained in or attached to it are private and confidential and intended exclusively for the addressee. MiTaller informs whoever may receive it in error that it contains confidential information whose use, copying, reproduction or distribution is prohibited. If you are not an intended recipient of this email, please notify the sender, delete it and do not read, act upon, print, disclose, copy, retain or redistribute any portion of it.</p>"),

            // Account
            ["verify.subject"] = ("Verificación de correo", "Email verification"),
            ["verify.intro"] = ("Este es tu código de verificación para acceder a la app MiTaller.", "This is your verification code to access the MiTaller app."),
            ["reset.subject"] = ("Recuperación de contraseña", "Password recovery"),
            ["reset.intro"] = ("Este es tu código de verificación para recuperar tu contraseña y que puedas acceder a la app MiTaller.", "This is your verification code to reset your password and access the MiTaller app."),
            ["account.created.title"] = ("Cuenta creada exitosamente", "Account created"),
            ["account.created.content"] = ("¡Tu cuenta ha sido creada con éxito! Ahora puedes acceder a todas las funcionalidades de tu cuenta.", "Your account has been created! You can now use every feature of your account."),
            ["welcome.subject"] = ("¡Bienvenid@ a MiTaller!", "Welcome to MiTaller!"),
            ["welcome.customerHtml"] = ("Hola <strong>{0}</strong>. Te damos la bienvenida a MiTaller. Tu cuenta ha sido creada por el taller <strong>{1}</strong>, al que llevaste tu vehículo para revisión o servicio.", "Hi <strong>{0}</strong>, welcome to MiTaller. Your account was created by <strong>{1}</strong>, the shop where you took your vehicle for an inspection or service."),
            ["welcome.customerNext"] = ("Para comenzar a usar tu cuenta, descarga la aplicación de clientes y restablece tu contraseña.", "To start using your account, download the MiTaller customer app and reset your password."),
            ["welcome.employeeHtml"] = ("Hola <strong>{0}</strong>. Te damos la bienvenida a MiTaller. Tu cuenta ha sido creada por el taller <strong>{1}</strong>.", "Hi <strong>{0}</strong>, welcome to MiTaller. Your account was created by the shop <strong>{1}</strong>."),
            ["welcome.employeeNext"] = ("Para comenzar a usar tu cuenta, descarga la aplicación y restablece tu contraseña.", "To start using your account, download the app and reset your password."),

            // Appointments (customer)
            ["appt.created.title"] = ("{0} te ha agendado una cita para el {1} a las {2}", "{0} booked an appointment for you on {1} at {2}"),
            ["appt.created.content"] = ("Confirma tu asistencia desde la app para que el taller prepare tu cita.", "Confirm your attendance in the app so the shop can get ready for you."),
            ["appt.created.subject"] = ("Nueva cita agendada - confirma tu asistencia", "New appointment booked - please confirm your attendance"),
            ["appt.created.heading"] = ("Tienes una nueva cita", "You have a new appointment"),
            ["appt.created.sub"] = ("Ábrela en la app y toca \"Confirmar asistencia\" para que el taller la prepare.", "Open it in the app and tap \"Confirm attendance\" so the shop can get ready."),
            ["appt.reminder.title"] = ("Recordatorio: tu cita con {0} es el {1} a las {2}", "Reminder: your appointment with {0} is on {1} at {2}"),
            ["appt.reminder.content"] = ("Aún no la has confirmado. Si no la confirmas se cancelará automáticamente unas horas antes.", "You haven't confirmed it yet. If you don't, it will be canceled automatically a few hours before."),
            ["appt.reminder.subject"] = ("Recordatorio de tu cita - confírmala", "Appointment reminder - please confirm"),
            ["appt.reminder.heading"] = ("Tu cita es pronto", "Your appointment is coming up"),
            ["appt.reminder.sub"] = ("Confírmala desde la app; si no lo haces, se cancelará automáticamente unas horas antes.", "Confirm it in the app; otherwise it will be canceled automatically a few hours before."),
            ["appt.expiring.title"] = ("Tu cita con {0} del {1} a las {2} está por vencer", "Your appointment with {0} on {1} at {2} is about to expire"),
            ["appt.expiring.content"] = ("Te quedan unas horas para confirmarla. Si no la confirmas se cancelará automáticamente.", "You have a few hours left to confirm it. If you don't, it will be canceled automatically."),
            ["appt.expiring.subject"] = ("Tu cita está por vencer - confírmala", "Your appointment is about to expire - please confirm"),
            ["appt.expiring.heading"] = ("Tu cita está por vencer", "Your appointment is about to expire"),
            ["appt.expiring.sub"] = ("Confírmala desde la app antes de que se cancele automáticamente.", "Confirm it in the app before it is canceled automatically."),
            ["appt.canceled.title"] = ("Tu cita con {0} del {1} a las {2} fue cancelada", "Your appointment with {0} on {1} at {2} was canceled"),
            ["appt.canceled.byShop"] = ("Se canceló automáticamente porque el taller no la confirmó a tiempo. Puedes pedir otra o elegir otro taller.", "It was canceled automatically because the shop didn't confirm it in time. You can request another one or choose a different shop."),
            ["appt.canceled.byCustomer"] = ("Se canceló automáticamente porque no fue confirmada a tiempo. Agenda una nueva cuando quieras.", "It was canceled automatically because it wasn't confirmed in time. Book a new one whenever you like."),
            ["appt.canceled.subject"] = ("Tu cita fue cancelada", "Your appointment was canceled"),
            ["appt.canceled.heading"] = ("Cita cancelada", "Appointment canceled"),
            ["appt.canceled.sub"] = ("No fue confirmada a tiempo. Puedes agendar una nueva cuando gustes.", "It wasn't confirmed in time. You can book a new one whenever you like."),
            ["appt.shopConfirmed"] = ("{0} confirmó tu cita del {1} a las {2}", "{0} confirmed your appointment on {1} at {2}"),
            ["appt.shopCanceled"] = ("{0} canceló tu cita del {1} a las {2}", "{0} canceled your appointment on {1} at {2}"),

            // Appointments (workshop inbox)
            ["appt.requested"] = ("{0} ha solicitado una cita para la fecha del {1} a las {2}", "{0} requested an appointment on {1} at {2}"),
            ["appt.pending.title"] = ("{0} espera que confirmes su cita del {1} a las {2}", "{0} is waiting for you to confirm their appointment on {1} at {2}"),
            ["appt.pending.today"] = ("Confírmala hoy; si no, se cancelará automáticamente 2 horas antes.", "Confirm it today; otherwise it will be canceled automatically 2 hours before."),
            ["appt.pending.now"] = ("Confírmala ya; si no, se cancelará automáticamente 2 horas antes.", "Confirm it now; otherwise it will be canceled automatically 2 hours before."),
            ["appt.autoCanceled.title"] = ("La cita de {0} del {1} a las {2} se canceló", "{0}'s appointment on {1} at {2} was canceled"),
            ["appt.autoCanceled.bySelf"] = ("Se canceló automáticamente porque no la confirmaste a tiempo.", "It was canceled automatically because you didn't confirm it in time."),
            ["appt.autoCanceled.byCustomer"] = ("Se canceló automáticamente porque el cliente no la confirmó a tiempo.", "It was canceled automatically because the customer didn't confirm it in time."),
            ["appt.customerConfirmed"] = ("{0} confirmó su cita del {1} a las {2}", "{0} confirmed their appointment on {1} at {2}"),
            ["appt.customerCanceled"] = ("{0} canceló su cita del {1} a las {2}", "{0} canceled their appointment on {1} at {2}"),

            // Quotes
            ["quote.title"] = ("Cotización del vehículo {0} - {1} - {2} - {3}", "Quote for {0} {1} - {2} - {3}"),
            ["quote.statusChanged"] = ("Cambio de estatus en cotización", "Quote status changed"),
            ["quote.canceledByCustomer"] = ("La cotización ha sido cancelada por el cliente.", "The customer canceled the quote."),
            ["quote.approvedByCustomer"] = ("La cotización ha sido aprobada por el cliente.", "The customer approved the quote."),
            ["quote.updated"] = ("La cotización ha sido modificada, favor de revisarla.", "The quote has been updated. Please review it."),

            // Roadside assistance and inspections
            ["roadside.title"] = ("Asistencia vial: {0}", "Roadside assistance: {0}"),
            ["roadside.vehicle"] = ("Vehículo", "Vehicle"),
            ["roadside.location"] = ("Ubicación: {0}", "Location: {0}"),
            ["inspection.statusRequest.title"] = ("Solicitud de estatus de inspección", "Inspection status request"),
            ["inspection.statusRequest.details"] = ("El cliente {0} solicita estatus de inspección para su vehículo {1}.", "Customer {0} is asking for a status update on the inspection of their {1}."),
            ["inspection.updated.title"] = ("Su inspección tiene cambios", "Your inspection has updates"),

            // Community and support
            ["ticket.replied"] = ("Respondimos tu ticket #{0}", "We replied to your ticket #{0}"),
            ["ticket.resolved"] = ("Tu ticket #{0} está resuelto", "Your ticket #{0} is resolved"),
            ["ticket.closed"] = ("Tu ticket #{0} está cerrado", "Your ticket #{0} is closed"),
            ["idea.approved.title"] = ("Tu idea ya está en Comunidad", "Your idea is now in Community"),
            ["idea.approved.content"] = ("\"{0}\" ya se puede votar.", "\"{0}\" is now open for voting."),
            ["idea.rejected.title"] = ("Tu idea no se publicó", "Your idea was not published"),
            ["idea.implemented.title"] = ("¡Hicimos tu idea realidad!", "We made your idea happen!"),
            ["idea.implemented.content"] = ("\"{0}\" ya está disponible en MiTaller. Gracias por proponerla.", "\"{0}\" is now available in MiTaller. Thanks for suggesting it."),
            ["idea.merged.title"] = ("Unimos tu idea con otra igual", "We merged your idea with a matching one"),
            ["idea.merged.content"] = ("\"{0}\" ahora suma votos en \"{1}\".", "\"{0}\" now adds its votes to \"{1}\"."),
            ["idea.merged.note"] = ("Ya existía una idea igual: \"{0}\". Tus votos pasaron a esa idea.", "A matching idea already existed: \"{0}\". Your votes moved to that idea."),

            // Accidents
            ["accident.code"] = ("código {0}", "code {0}"),
            ["accident.contact.subject"] = ("🚨 Alerta de emergencia - {0}", "🚨 Emergency alert - {0}"),
            ["accident.contact.notif.title"] = ("Accidente reportado", "Accident reported"),
            ["accident.contact.notif.content"] = ("Reportaron un posible accidente del vehículo {0} de {1}.", "A possible accident involving {1}'s {0} was reported."),
            ["accident.owner.notif.title"] = ("Reportaron un accidente de tu vehículo", "An accident involving your vehicle was reported"),
            ["accident.owner.notif.content"] = ("Alguien reportó un posible accidente de tu {0}. Si estás bien, avísalo en la app para tranquilizar a tus contactos.", "Someone reported a possible accident involving your {0}. If you're OK, let us know in the app to reassure your contacts."),
            ["accident.owner.subject"] = ("🚨 Reportaron un accidente de tu vehículo, ¿estás bien?", "🚨 An accident involving your vehicle was reported. Are you OK?"),
            ["accident.locationHtml"] = ("📍 <strong>Ubicación del reporte:</strong>", "📍 <strong>Report location:</strong>"),
            ["accident.viewMap"] = ("Ver en Google Maps", "View on Google Maps"),
            ["accident.whenHtml"] = ("🕓 <strong>Fecha y hora del reporte:</strong>", "🕓 <strong>Report date and time:</strong>"),
            ["accident.dear"] = ("Estimado(a) {0},", "Dear {0},"),
            ["accident.hello"] = ("Hola {0},", "Hi {0},"),
            ["accident.contact.introHtml"] = ("Eres el contacto de emergencia registrado por <strong>{0}</strong>. Una persona reportó desde la app MiTaller un posible accidente de su vehículo <strong>{1}</strong>.", "You are the emergency contact registered by <strong>{0}</strong>. Someone used the MiTaller app to report a possible accident involving their vehicle <strong>{1}</strong>."),
            ["accident.contact.adviceHtml"] = ("Este mensaje no necesariamente indica una situación grave, pero te recomendamos contactar a <strong>{0}</strong> para confirmar su estado.", "This message doesn't necessarily mean the situation is serious, but we recommend contacting <strong>{0}</strong> to check on them."),
            ["accident.owner.introHtml"] = ("Una persona reportó desde la app MiTaller un posible accidente de tu vehículo <strong>{0}</strong>.", "Someone used the MiTaller app to report a possible accident involving your vehicle <strong>{0}</strong>."),
            ["accident.owner.noContacts"] = ("No tienes contactos de emergencia que reciban avisos. Puedes agregarlos en la app, en tu perfil.", "You have no emergency contacts set to receive alerts. You can add them in the app, in your profile."),
            ["accident.owner.oneContact"] = ("Ya avisamos a tu contacto de emergencia.", "We already alerted your emergency contact."),
            ["accident.owner.manyContacts"] = ("Ya avisamos a tus {0} contactos de emergencia.", "We already alerted your {0} emergency contacts."),
            ["accident.owner.ifOk"] = ("Si estás bien y fue un error, avísales:", "If you're OK and it was a mistake, let them know:"),
            ["accident.imOk"] = ("Estoy bien, fue un error", "I'm OK, it was a mistake"),
            ["accident.falseAlarm.subject"] = ("✅ Falsa alarma - {0} está bien", "✅ False alarm - {0} is OK"),
            ["accident.falseAlarm.notif.title"] = ("Falsa alarma", "False alarm"),
            ["accident.falseAlarm.notif.content"] = ("{0} confirmó que está bien: el accidente reportado de su {1} fue un error.", "{0} confirmed they're OK: the reported accident involving their {1} was a mistake."),
            ["accident.falseAlarm.bodyHtml"] = ("<strong>{0}</strong> confirmó que está bien: el posible accidente de su vehículo <strong>{1}</strong> que te avisamos el {2} a las {3} fue una falsa alarma.", "<strong>{0}</strong> confirmed they're OK: the possible accident involving their vehicle <strong>{1}</strong> that we told you about on {2} at {3} was a false alarm."),
            ["accident.falseAlarm.thanks"] = ("Gracias por estar al pendiente.", "Thank you for looking out for them."),
            ["accident.page.invalid.title"] = ("Enlace no válido", "Invalid link"),
            ["accident.page.invalid.body"] = ("Este enlace no es válido o ya expiró.", "This link is not valid or has expired."),
            ["accident.page.already.title"] = ("Ya avisamos a tus contactos", "We already told your contacts"),
            ["accident.page.already.body"] = ("Tus contactos de emergencia ya recibieron el aviso de que fue una falsa alarma.", "Your emergency contacts were already told it was a false alarm."),
            ["accident.page.ask.title"] = ("¿Estás bien?", "Are you OK?"),
            ["accident.page.ask.bodyHtml"] = ("Reportaron un posible accidente de tu <strong>{0}</strong> el {1} a las {2}.", "A possible accident involving your <strong>{0}</strong> was reported on {1} at {2}."),
            ["accident.page.ask.confirm"] = ("Si estás bien, confirma y avisaremos a tus contactos de emergencia que fue una falsa alarma.", "If you're OK, confirm and we'll tell your emergency contacts it was a false alarm."),
            ["accident.page.done.title"] = ("Listo, qué bueno que estás bien", "Done, we're glad you're OK"),
            ["accident.page.done.body"] = ("Avisamos a tus contactos de emergencia que fue una falsa alarma.", "We told your emergency contacts it was a false alarm."),

            // Inspection PDF
            ["pdf.inspectionOf"] = ("Inspección de {0}", "{0} inspection"),
            ["pdf.info.motorcycle"] = ("Información de la motocicleta", "Motorcycle information"),
            ["pdf.info.watercraft"] = ("Información de la embarcación", "Watercraft information"),
            ["pdf.info.aircraft"] = ("Información de la aeronave", "Aircraft information"),
            ["pdf.info.vehicle"] = ("Información del vehículo", "Vehicle information"),
            ["pdf.plates"] = ("Placas/Matrícula", "Plates/Registration"),
            ["pdf.year"] = ("Año", "Year"),
            ["pdf.make"] = ("Marca", "Make"),
            ["pdf.model"] = ("Modelo", "Model"),
            ["pdf.trim"] = ("Versión", "Trim"),
            ["pdf.type"] = ("Tipo", "Type"),
            ["pdf.customerInfo"] = ("Información del cliente", "Customer information"),
            ["pdf.shopInfo"] = ("Información del taller", "Shop information"),
            ["pdf.name"] = ("Nombre", "Name"),
            ["pdf.phone"] = ("Número de teléfono", "Phone number"),
            ["pdf.customerNumber"] = ("Número de cliente", "Customer number"),
            ["pdf.shopNumber"] = ("Número de taller", "Shop number"),
            ["pdf.generalObservations"] = ("Observaciones generales", "General observations"),
            ["pdf.optimal"] = ("Óptimo", "Good"),
            ["pdf.needsAttention"] = ("Requiere atención", "Needs attention"),
            ["pdf.urgent"] = ("Atención urgente", "Urgent attention"),
            ["pdf.notApplicable"] = ("No aplica", "N/A"),
            ["pdf.generatedBy"] = ("Generado por MiTaller", "Generated by MiTaller"),
            ["pdf.page"] = ("Página ", "Page "),
            ["pdf.of"] = (" de ", " of "),
            ["pdf.photos"] = ("Fotos del vehículo", "Vehicle photos"),
            ["pdf.disclaimer"] = ("Este reporte refleja el estado del vehículo únicamente al momento de la inspección y tiene fines informativos; no constituye una garantía ni un diagnóstico definitivo de fallas futuras.", "This report reflects the vehicle's condition only at the time of the inspection and is for information purposes; it is not a warranty or a definitive diagnosis of future failures."),
        };
    }
}
