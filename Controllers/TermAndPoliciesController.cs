using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MiTaller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TermAndPoliciesController : ControllerBase
    {
        [HttpGet("Terms")]
        public IActionResult GetTerms()
        {
            try {
                var terminosJson = new
                {
                    titulo = "TÉRMINOS Y CONDICIONES DE USO",
                    secciones = new List<object>
                    {
                        new
                        {
                            tipo = "texto",
                            valor = "Última actualización: 18 Marzo 2025"
                        },
                        new
                        {
                            titulo = "1. IDENTIFICACIÓN DEL RESPONSABLE",
                            contenido = new List<object>
                            {
                                new {
                                    tipo = "texto",
                                    valor = "El presente documento establece los términos y condiciones bajo los cuales Mi Taller (la Plataforma), " +
                                    "operada por Mi Taller (Nosotros), ofrece sus servicios a los usuarios (Usuarios) en México."
                                },
                                new {
                                    tipo = "texto",
                                    valor = "Para cualquier duda, los usuarios pueden contactarnos en:"
                                },
                                new {
                                    tipo = "lista",
                                    elementos = new List<string>
                                    {
                                        "Correo electrónico: info@mitaller.io",
                                        "Domicilio: Calle Yuca, Casa 54, Colonia Las vegas, Texcoco Estado de México.",
                                    }
                                },
                            }
                        },
                        new
                        {
                            titulo = "2. ACEPTACIÓN DE LOS TÉRMINOS Y CONDICIONES",
                            contenido = new List<object>
                            {
                                new {
                                    tipo = "texto",
                                    valor = "Al registrarse en Mi Taller, los Usuarios aceptan haber leído y entendido estos términos y se obligan a cumplirlos. " +
                                    "Si no están de acuerdo, deberán abstenerse de usar la plataforma."
                                }
                            }
                        },
                        new
                        {
                            titulo = "3. SERVICIOS OFRECIDOS",
                            contenido = new List<object>
                            {
                                new {
                                    tipo = "texto",
                                    valor = "Mi Taller ofrece a los talleres mecánicos y sus clientes una solución digital para la gestión de talleres, incluyendo:"
                                },
                                new {
                                    tipo = "lista",
                                    elementos = new List<string>
                                    {
                                        "Administración de ingresos y egresos del taller.",
                                        "Solicitud de créditos a instituciones financieras afiliadas (Fintechs).",
                                        "Gestión de clientes y vehículos.",
                                        "Envío de cotizaciones y comunicación con clientes.",
                                        "Venta de espacios publicitarios dentro de la plataforma para la promoción de servicios de los talleres.",
                                        "Integración con aseguradoras y refaccionarias en el futuro."
                                    }
                                },
                                new {
                                    tipo = "texto",
                                    valor = "Los servicios podrán modificarse o ampliarse sin previo aviso, según lo determine Mi Taller."
                                }
                            }
                        },
                        new
                        {
                            titulo = "4. REQUISITOS PARA EL USO DE LA PLATAFORMA",
                            contenido = new List<object>
                            {
                                new {
                                    tipo = "texto",
                                    valor = "Para acceder a Mi Taller, los Usuarios deben:"
                                },
                                new {
                                    tipo = "lista",
                                    elementos = new List<string>
                                    {
                                        "Ser mayores de 18 años.",
                                        "Proporcionar información verídica y actualizada.",
                                        "No utilizar la plataforma con fines ilícitos."
                                    }
                                },
                                new {
                                    tipo = "texto",
                                    valor = "Mi Taller se reserva el derecho de suspender o cancelar cuentas si detecta información falsa o uso indebido."
                                }
                            }
                        },
                        new
                        {
                            titulo = "5. RESPONSABILIDADES DEL USUARIO",
                            contenido = new List<object>
                            {
                                new {
                                    tipo = "texto",
                                    valor = "Los Usuarios se comprometen a:"
                                },
                                new {
                                    tipo = "lista",
                                    elementos = new List<string>
                                    {
                                        "Usar la plataforma conforme a la ley y estos términos.",
                                        "No compartir su cuenta con terceros.",
                                        "No intentar acceder a información de otros usuarios sin autorización.",
                                        "Mantener actualizada su información en la plataforma."
                                    }
                                },
                                new {
                                    tipo = "texto",
                                    valor = "El incumplimiento de estas responsabilidades podrá resultar en la suspensión o cancelación de la cuenta."
                                }
                            }
                        },
                        new
                        {
                            titulo = "6. SOLICITUD DE CRÉDITOS Y RELACIÓN CON FINTECHS",
                            contenido = new List<object>
                            {
                                new {
                                    tipo = "texto",
                                    valor = "La plataforma permite a los talleres solicitar créditos a través de Fintechs afiliadas."
                                },
                                new {
                                    tipo = "lista",
                                    elementos = new List<string>
                                    {
                                        "Mi Taller no es una institución financiera y solo actúa como intermediario tecnológico.",
                                        "La aprobación de créditos depende exclusivamente de las Fintechs, según sus políticas y análisis de riesgo.",
                                        "Al solicitar un crédito, el usuario autoriza la compartición de su información con las Fintechs seleccionadas.",
                                    }
                                },
                                new {
                                    tipo = "texto",
                                    valor = "Cláusula de Exoneración: Mi Taller no es responsable por el otorgamiento o rechazo de créditos, ni por los términos aplicados por las Fintechs."
                                }
                            }
                        },
                        new
                        {
                            titulo = "7. PRIVACIDAD Y USO DE DATOS",
                            contenido = new List<object>
                            {
                                new {
                                    tipo = "texto",
                                    valor = "El tratamiento de los datos personales se rige por nuestro Aviso de Privacidad."
                                },
                                new {
                                    tipo = "texto",
                                    valor = "Principales consideraciones:"
                                },
                                new {
                                    tipo = "lista",
                                    elementos = new List<string>
                                    {
                                        "Los datos pueden ser compartidos con Fintechs, aseguradoras, refaccionarias y otros terceros para ofrecer los servicios.",
                                        "Los Usuarios pueden ejercer sus derechos ARCO (Acceso, Rectificación, Cancelación y Oposición) conforme a la Ley Federal de Protección de Datos Personales en Posesión de los Particulares.",
                                    }
                                },
                                new {
                                    tipo = "texto",
                                    valor = "Mi Taller implementa medidas de seguridad para proteger la información, pero no se hace responsable por vulneraciones de seguridad fuera de su control."
                                },
                            }
                        },
                        new
                        {
                            titulo = "8. COSTOS, SUSCRIPCIONES Y ANUNCIOS",
                            contenido = new List<object>
                            {
                                new {
                                    tipo = "lista",
                                    elementos = new List<string>
                                    {
                                        "El registro en Mi Taller es gratuito.",
                                        "Se aplicará una suscripción cuando el taller registre más de 16 clientes en la plataforma.",
                                        "Los costos de suscripción serán comunicados en la plataforma y podrán cambiar sin previo aviso.",
                                        "Mi Taller también ofrece la venta de espacios publicitarios dentro de la plataforma, para que los talleres promocionen sus servicios a otros usuarios.",
                                        "Los precios y condiciones de los anuncios serán establecidos por Mi Taller y podrán modificarse en cualquier momento.",
                                    }
                                },
                                new {
                                    tipo = "texto",
                                    valor = "Mi Taller se reserva el derecho de suspender servicios en caso de falta de pago, tanto por suscripción como por la contratación de anuncios."
                                },
                            }
                        },
                        new
                        {
                            titulo = "9. PROPIEDAD INTELECTUAL",
                            contenido = new List<object>
                            {
                                new {
                                    tipo = "texto",
                                    valor = "Todos los derechos sobre la plataforma, su diseño, código y contenido pertenecen a Mi Taller."
                                },
                                new {
                                    tipo = "lista",
                                    elementos = new List<string>
                                    {
                                        "Los Usuarios no pueden copiar, modificar o distribuir el software sin autorización.",
                                        "Se prohíbe el uso de la marca o logotipos sin permiso escrito.",
                                    }
                                },
                                new {
                                    tipo = "texto",
                                    valor = "Cualquier violación a estos derechos será sancionada conforme a la Ley Federal del Derecho de Autor y otras normativas aplicables."
                                }
                            }
                        },
                        new
                        {
                            titulo = "10. LIMITACIÓN DE RESPONSABILIDAD",
                            contenido = new List<object>
                            {
                                new {
                                    tipo = "texto",
                                    valor = "Mi Taller no se hace responsable por:"
                                },
                                new {
                                    tipo = "lista",
                                    elementos = new List<string>
                                    {
                                        "Errores o interrupciones en la plataforma.",
                                        "Pérdidas económicas derivadas del uso del servicio.",
                                        "Incumplimientos por parte de Fintechs, aseguradoras u otros terceros.",
                                        "Decisiones tomadas por los Usuarios con base en la información de la plataforma.",
                                        "Resultados obtenidos por la contratación de anuncios dentro de la plataforma."
                                    }
                                }
                            }
                        },
                        new
                        {
                            titulo = "11. CANCELACIÓN Y ELIMINACIÓN DE DATOS",
                            contenido = new List<object>
                            {
                                new {
                                    tipo = "texto",
                                    valor = "Los Usuarios pueden cancelar su cuenta en cualquier momento enviando una solicitud a info@mitaller.io"
                                },
                                new {
                                    tipo = "lista",
                                    elementos = new List<string>
                                    {
                                        "Mi Taller eliminará los datos conforme a las regulaciones aplicables, excepto cuando haya una obligación legal de conservarlos.",
                                        "Si el Usuario tiene una deuda con una Fintech afiliada, la cancelación de cuenta no anula sus obligaciones con esa entidad.",
                                    }
                                }
                            }
                        },
                        new
                        {
                            titulo = "12. MODIFICACIONES A LOS TÉRMINOS Y CONDICIONES",
                            contenido = new List<object>
                            {
                                new {
                                    tipo = "texto",
                                    valor = "Mi Taller podrá modificar estos términos en cualquier momento. Los cambios serán notificados mediante la plataforma o correo electrónico."
                                },
                                new {
                                    tipo = "texto",
                                    valor = "El uso continuado de la plataforma implica la aceptación de los cambios."
                                },
                            }
                        },
                        new
                        {
                            titulo = "13. JURISDICCIÓN Y LEGISLACIÓN APLICABLE",
                            contenido = new List<object>
                            {
                                new {
                                    tipo = "texto",
                                    valor = "Estos términos se rigen por las leyes de México."
                                },
                                new {
                                    tipo = "lista",
                                    elementos = new List<string>
                                    {
                                        "Cualquier disputa será resuelta ante los tribunales del Estado de México",
                                    }
                                },
                                new {
                                    tipo = "texto",
                                    valor = "Mi Taller promueve la resolución de conflictos de manera amistosa antes de recurrir a instancias legales."
                                },
                            }
                        },
                        new
                        {
                            titulo = "14. CONTACTO Y SOPORTE",
                            contenido = new List<object>
                            {
                                new {
                                    tipo = "texto",
                                    valor = "Para dudas o aclaraciones, los Usuarios pueden contactarnos en:"
                                },
                                new {
                                    tipo = "lista",
                                    elementos = new List<string>
                                    {
                                        "Correo electrónico: info@mitaller.io",
                                        "Domicilio: Calle Yuca, Casa 54, Colonia Las vegas, Texcoco Estado de México.",
                                    }
                                },
                            }
                        }
                    }
                };

                return Ok(terminosJson);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("Privacy")]
        public IActionResult GetPrivacyPolicy()
        {
            try
            {
                var avisoPrivacidadJson = new
                {
                    titulo = "AVISO DE PRIVACIDAD INTEGRAL",
                    secciones = new List<object>
                    {
                        new
                        {
                            titulo = "MI TALLER – PLATAFORMA DIGITAL PARA TALLERES MECÁNICOS Y CLIENTES DE LOS TALLERES.",
                        },
                        new
                        {
                            titulo = "1. IDENTIDAD Y DOMICILIO DEL RESPONSABLE",
                            contenido = new List<object>
                            {
                                new {
                                    tipo = "texto",
                                    valor = "MI TALLER S.A. DE C.V. (en adelante, Mi Taller), con domicilio en Calle Yuca, Casa 54, Colonia Las vegas, " +
                                    "Texcoco Estado de México, es responsable del tratamiento de sus datos personales conforme a los términos del presente Aviso de Privacidad."
                                },
                                new {
                                    tipo = "texto",
                                    valor = "Para cualquier duda o solicitud en relación con sus datos personales, puede contactarnos en info@mitaller.io."
                                }
                            }
                        },
                        new
                        {
                            titulo = "2. DATOS PERSONALES RECABADOS",
                            contenido = new List<object>
                            {
                                new {
                                    tipo = "texto",
                                    valor = "Para la prestación de nuestros servicios, Mi Taller podrá recabar las siguientes categorías de datos personales:"
                                },
                                new {
                                    tipo = "lista",
                                    elementos = new List<string>
                                    {
                                        "Datos de identificación: Nombre completo, CURP, RFC, identificación oficial (INE/Pasaporte), fecha de nacimiento.",
                                        "Datos de contacto: Teléfono, correo electrónico, domicilio.",
                                        "Datos financieros y patrimoniales: Información de cuentas bancarias, historial crediticio, ingresos, egresos, estados financieros.",
                                        "Datos de geolocalización: Ubicación del dispositivo en tiempo real (si se otorga consentimiento explícito en la app).",
                                        "Datos sobre su taller mecánico: Nombre comercial, razón social, ubicación, registros fiscales, clientes atendidos."
                                    }
                                },
                                new {
                                    tipo = "texto",
                                    valor = "Mi Taller no recaba datos personales sensibles, como datos de salud, religión, origen étnico, orientación sexual, entre otros."
                                }
                            }
                        },
                        new
                        {
                            titulo = "3. FINALIDADES DEL TRATAMIENTO DE DATOS",
                            contenido = new List<object>
                            {
                                new {
                                    tipo = "texto",
                                    valor = "Los datos personales recabados serán utilizados para las siguientes finalidades:"
                                },
                                new {
                                    tipo = "subtitulo",
                                    valor = "Finalidades Primarias (necesarias para el servicio)"
                                },
                                new {
                                    tipo = "lista",
                                    elementos = new List<string>
                                    {
                                        "Creación y administración de su cuenta en Mi Taller..",
                                        "Registro y gestión de su taller mecánico dentro de la plataforma.",
                                        "Conexión con clientes, aseguradoras, refaccionarias y fintechs para ofrecer servicios financieros y comerciales.",
                                        "Gestión de solicitudes de crédito con fintechs aliadas.",
                                        "Evaluación de su historial crediticio en caso de solicitar financiamiento.",
                                        "Facturación y cobro de suscripciones o transacciones realizadas en la plataforma.",
                                        "Prevención de fraude, lavado de dinero y financiamiento ilícito.",
                                        "Cumplimiento de obligaciones fiscales y regulatorias."
                                    }
                                },
                                new {
                                    tipo = "subtitulo",
                                    valor = "Finalidades Secundarias (opcionales)"
                                },
                                new {
                                    tipo = "lista",
                                    elementos = new List<string>
                                    {
                                        "Envío de publicidad y promociones.",
                                        "Análisis de datos para mejorar la experiencia del usuario.",
                                        "Encuestas de satisfacción y estudios de mercado."
                                    }
                                },
                                new {
                                    tipo = "texto",
                                    valor = "Si no desea que sus datos sean utilizados para finalidades secundarias, puede solicitarlo enviando un correo a info@mitaller.io"
                                },
                            }
                        },
                        new
                        {
                            titulo = "4. TRANSFERENCIA DE DATOS PERSONALES",
                            contenido = new List<object>
                            {
                                new {
                                    tipo = "texto",
                                    valor = "Sus datos personales podrán ser compartidos con los siguientes terceros, únicamente para los fines aquí descritos:"
                                },
                                new {
                                    tipo = "lista",
                                    elementos = new List<string>
                                    {
                                        "Instituciones Financieras y Fintechs: Para procesar solicitudes de crédito o financiamiento.",
                                        "Aseguradoras: Para cotización de pólizas y seguros vehiculares.",
                                        "Refaccionarias y proveedores de autopartes: Para la compra de refacciones y materiales para su taller.",
                                        "Autoridades Fiscales y Regulatorias: En cumplimiento de obligaciones legales y fiscales."
                                    }
                                },
                                new {
                                    tipo = "texto",
                                    valor = "Las transferencias de datos se realizan bajo estrictas medidas de seguridad y en cumplimiento de la LFPDPPP."
                                }
                            }
                        },
                        new
                        {
                            titulo = "5. BASE LEGAL DEL TRATAMIENTO",
                            contenido = new List<object>
                            {
                                new {
                                    tipo = "texto",
                                    valor = "El tratamiento de sus datos se realiza con base en:"
                                },
                                new {
                                    tipo = "lista",
                                    elementos = new List<string>
                                    {
                                        "Consentimiento expreso otorgado por el usuario al registrarse en la plataforma.",
                                        "Obligaciones contractuales derivadas del uso de los servicios de Mi Taller.",
                                        "Intereses legítimos en la prevención de fraude y seguridad de la plataforma.",
                                        "Obligaciones legales aplicables a Mi Taller y sus aliados comerciales."
                                    }
                                }
                            }
                        },
                        new
                        {
                            titulo = "6. MEDIDAS DE SEGURIDAD",
                            contenido = new List<object>
                            {
                                new {
                                    tipo = "texto",
                                    valor = "Para proteger sus datos personales, Mi Taller implementa medidas de seguridad administrativas, técnicas y físicas, incluyendo:"
                                },
                                new {
                                    tipo = "lista",
                                    elementos = new List<string>
                                    {
                                        "Encriptación de datos sensibles.",
                                        "Acceso restringido a la información solo a personal autorizado.",
                                        "Almacenamiento seguro en servidores con protocolos de seguridad avanzada"
                                    }
                                }
                            }
                        },
                        new
                        {
                            titulo = "7. DERECHOS ARCO (Acceso, Rectificación, Cancelación y Oposición)",
                            contenido = new List<object>
                            {
                                new {
                                    tipo = "texto",
                                    valor = "Usted tiene derecho a:"
                                },
                                new {
                                    tipo = "lista",
                                    elementos = new List<string>
                                    {
                                        "Acceder a sus datos personales en posesión de Mi Taller.",
                                        "Rectificar datos inexactos o incompletos.",
                                        "Cancelar sus datos cuando considere que no son necesarios para las finalidades descritas.",
                                        "Oponerse al uso de sus datos para fines secundarios o comerciales."
                                    }
                                },
                                new {
                                    tipo = "texto",
                                    valor = "Para ejercer estos derechos, envíe una solicitud al correo info@mitaller.io con la siguiente información:"
                                },
                                new {
                                    tipo = "lista",
                                    elementos = new List<string>
                                    {
                                        "•\tNombre completo.",
                                        "•\tCopia de identificación oficial.",
                                        "•\tDescripción clara del derecho que desea ejercer."
                                    }
                                },
                                new {
                                    tipo = "texto",
                                    valor = "Las solicitudes serán atendidas en un plazo máximo de 20 días hábiles."
                                }
                            }
                        },
                        new
                        {
                            titulo = "8. REVOCACIÓN DEL CONSENTIMIENTO Y ELIMINACIÓN DE CUENTA",
                            contenido = new List<object>
                            {
                                new {
                                    tipo = "texto",
                                    valor = "Si desea revocar su consentimiento para el tratamiento de sus datos o eliminar su cuenta de Mi Taller, puede solicitarlo en info@mitaller.io"
                                },
                                new {
                                    tipo = "texto",
                                    valor = "Los datos se eliminarán en un plazo de 30 días hábiles, salvo que exista una obligación legal para su conservación."
                                }
                            }
                        },
                        new
                        {
                            titulo = "9. USO DE COOKIES Y TECNOLOGÍAS SIMILARES",
                            contenido = new List<object>
                            {
                                new {
                                    tipo = "texto",
                                    valor = "Mi Taller puede utilizar cookies y tecnologías de seguimiento para mejorar la experiencia del usuario en la plataforma. Puede deshabilitar las cookies desde la configuración de su navegador."
                                }
                            }
                        },
                        new
                        {
                            titulo = "10. CAMBIOS AL AVISO DE PRIVACIDAD",
                            contenido = new List<object>
                            {
                                new {
                                    tipo = "texto",
                                    valor = "Mi Taller se reserva el derecho de modificar este aviso de privacidad en cualquier momento. Los cambios serán notificados a través de nuestra plataforma y/o correo electrónico."
                                }
                            }
                        },
                        new
                        {
                            titulo = "11. JURISDICCIÓN Y LEYES APLICABLES",
                            contenido = new List<object>
                            {
                                new {
                                    tipo = "texto",
                                    valor = "Este Aviso de Privacidad se rige por la Ley Federal de Protección de Datos Personales en Posesión de los Particulares (LFPDPPP) y demás normativas aplicables en México."
                                },
                                new {
                                    tipo = "texto",
                                    valor = "En caso de controversia, las partes se someten a la jurisdicción de los tribunales del Estado de México renunciando a cualquier otra jurisdicción que pudiera corresponderles."
                                }
                            }
                        },
                        new
                        {
                            titulo = "12. CONTACTO",
                            contenido = new List<object>
                            {
                                new {
                                    tipo = "texto",
                                    valor = "Para cualquier duda o aclaración sobre este Aviso de Privacidad, puede contactarnos en:"
                                },
                                new {
                                    tipo = "lista",
                                    elementos = new List<string>
                                    {
                                        "Correo electrónico: info@mitaller.io",
                                        "Domicilio: Calle Yuca, Casa 54, Colonia Las vegas, Texcoco Estado de México.",
                                    }
                                },
                                new {
                                    tipo = "texto",
                                    valor = "Fecha de última actualización: 18 Marzo 2025"
                                }
                            }
                        }
                    }
                };


                return Ok(avisoPrivacidadJson);
            }
            catch (Exception)
            {
                return BadRequest("unknown-error");
            }
        }
    }
}
