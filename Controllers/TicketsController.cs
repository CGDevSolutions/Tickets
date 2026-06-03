using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Text.Json;
using Tickets.Data;
using Tickets.DTOs;
using Tickets.Models;
using Tickets.Services;
using Tickets.Servicios;

namespace Tickets.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly LogService _logService;
        private readonly AuditoriaService _auditoriaService;
        private readonly CorreoService _correoService;
        private readonly IWebHostEnvironment _environment;

        
public TicketsController(
    AppDbContext context,
    LogService logService,
    AuditoriaService auditoriaService,
    CorreoService correoService,
    IWebHostEnvironment environment
)
        {
            _context = context;
            _logService = logService;
            _auditoriaService = auditoriaService;
            _correoService = correoService;
            _environment = environment;
        }



        // =========================================
        // ADMINISTRADOR Y TECNICO
        // VER TODOS LOS TICKETS
        // =========================================

        [Authorize(Roles = "Administrador,Tecnico")]
        [HttpGet]
        public IActionResult Obtener()
        {
            var tickets = _context.Tickets
                .Select(t => new
                {
                    t.Id,
                    t.Codigo,
                    t.Titulo,
                    t.Descripcion,
                    t.Estado,
                    t.Prioridad,
                    t.FechaCreacion,
                    t.Tipo,
                    t.Grupo,

                    Archivos = _context.ArchivosTicket
                        .Where(a => a.TicketId == t.Id)
                        .Select(a => new
                        {
                            a.Id,
                            a.NombreArchivo,
                            a.RutaArchivo
                        })
                        .ToList(),

                    Usuario = new
                    {
                        t.Usuario.Id,
                        t.Usuario.Nombre,
                        t.Usuario.Correo,
                        t.Usuario.Rol
                    },

                    Tecnico = t.TecnicoId != null
                    ? new
                    {
                        t.Tecnico.Id,
                        t.Tecnico.Nombre,
                        t.Tecnico.Rol
                    }
                    : null
                })
                .OrderByDescending(x => x.FechaCreacion)
                .ToList();

            return Ok(tickets);
        }

        // =========================================
        // OBTENER TECNICOS Y ADMINS
        // =========================================

        [Authorize(Roles = "Administrador")]
        [HttpGet("usuarios-asignacion")]
        public IActionResult UsuariosAsignacion()
        {
            var usuarios = _context.Usuarios
                .Where(x =>
                    x.Rol == "Administrador"
                    || x.Rol == "Tecnico"
                )
                .Select(x => new
                {
                    x.Id,
                    x.Nombre,
                    x.Rol
                })
                .ToList();

            return Ok(usuarios);
        }

        // =========================================
        // USUARIO
        // VER SOLO SUS TICKETS
        // =========================================

        [Authorize(Roles = "Usuario")]
        [HttpGet("mis-tickets")]
        public IActionResult MisTickets()
        {
            var usuarioId = int.Parse(
                User.FindFirst("UsuarioId")!.Value
            );

            var tickets = _context.Tickets
                .Where(t => t.UsuarioId == usuarioId)
                .Select(t => new
                {
                    t.Id,

                    t.Codigo,
                    t.Titulo,
                    t.Descripcion,
                    t.Estado,
                    t.Prioridad,
                    t.Tipo,
                    t.Grupo,
                    t.FechaCreacion,

                    Archivos = _context.ArchivosTicket
                        .Where(a => a.TicketId == t.Id)
                        .Select(a => new
                        {
                            id = a.Id,
                            nombreArchivo = a.NombreArchivo,
                            rutaArchivo = a.RutaArchivo
                        })
                        .ToList()
                })
                .OrderByDescending(t => t.FechaCreacion)
                .ToList();

            return Ok(tickets);
        }

        // =========================================
        // VER TICKET POR ID
        // =========================================

        [HttpGet("{id}")]
        public IActionResult ObtenerPorId(int id)
        {
            var ticket = _context.Tickets
                .FirstOrDefault(x => x.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            return Ok(ticket);
        }

        // =========================================
        // CAMBIAR PRIORIDAD
        // =========================================

[Authorize(Roles = "Administrador")]
[HttpPut("prioridad/{id}")]
public async Task<IActionResult> CambiarPrioridad(
    int id,
    [FromBody] JsonElement body)
        {
            var ticket = _context.Tickets
                .FirstOrDefault(x => x.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            // COPIA ANTES DEL CAMBIO

            var ticketAnterior = new Ticket
            {
                Prioridad = ticket.Prioridad
            };

            var prioridad = body.GetString();

            ticket.Prioridad = prioridad;

            _context.SaveChanges();

            // AUDITORÍA

            await _auditoriaService.RegistrarCambios(
                "Tickets",
                ticket.Id,
                ticketAnterior,
                ticket
            );

            return Ok(ticket);
        }


        // =========================================
        // CREAR TICKET
        // =========================================

        [Authorize(Roles = "Administrador,Usuario")]
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearTicketDTO dto)
        {
            var usuarioId = int.Parse(
                User.FindFirst("UsuarioId")!.Value
            );

            var usuario = _context.Usuarios
                .FirstOrDefault(x => x.Id == usuarioId);

            if (usuario == null)
            {
                return BadRequest("Usuario no encontrado");
            }

            var ticket = new Ticket
            {
                Titulo = dto.Titulo,
                Descripcion = dto.Descripcion,
                Prioridad = dto.Prioridad,
                Tipo = dto.Tipo,
                Grupo = dto.Grupo,

                UsuarioId = usuarioId,

                Codigo = "",

                Estado = "Nuevo",

                FechaCreacion = DateTime.Now
            };

            _context.Tickets.Add(ticket);

            _context.SaveChanges();

            // =========================================
            // REGISTRAR LOG
            // =========================================
            await _logService.RegistrarLog(
    $"Creó ticket #{ticket.Id}"

);

            // =========================================
            // GENERAR CODIGO CORTO
            // =========================================

            ticket.Codigo = $"TK-{ticket.Id.ToString("D4")}";

            _context.SaveChanges();

            // =========================================
            // CREAR NOTIFICACIONES
            // =========================================

            // NOTIFICACION USUARIO

            _context.Notificaciones.Add(new Notificacion
            {
                UsuarioId = usuario.Id,

                Titulo = "Ticket creado",

                Mensaje =
                    $"Tu ticket {ticket.Codigo} fue creado correctamente.",

                LinkUrl = $"/tickets/{ticket.Id}"
            });

            // NOTIFICACIONES ADMINS Y TECNICOS

            var usuariosNotificar = _context.Usuarios
                .Where(x =>
                    x.Rol == "Administrador"
                    || x.Rol == "Tecnico"
                )
                .ToList();

            foreach (var destino in usuariosNotificar)
            {
                _context.Notificaciones.Add(new Notificacion
                {
                    UsuarioId = destino.Id,

                    Titulo = "Nuevo ticket registrado",

                    Mensaje =
                        $"{usuario.Nombre} creó el ticket {ticket.Codigo}",

                    LinkUrl = $"/tickets/{ticket.Id}"
                });
            }

            _context.SaveChanges();

            // =========================================
            // ENVIAR CORREOS
            // =========================================

            try
            {
                // CORREO USUARIO

                if (!string.IsNullOrEmpty(usuario.Correo))
                {
                    _correoService.EnviarCorreo(
                        usuario.Correo,
                        $"Ticket creado correctamente - {ticket.Codigo}",
                        $@"
                        <h2>Nuevo Ticket Creado</h2>

                        <p><b>Código:</b> {ticket.Codigo}</p>
                        <p><b>Título:</b> {ticket.Titulo}</p>
                        <p><b>Descripción:</b> {ticket.Descripcion}</p>
                        <p><b>Prioridad:</b> {ticket.Prioridad}</p>
                        <p><b>Estado:</b> {ticket.Estado}</p>

                        <br>

                        <p>Tu ticket fue registrado correctamente.</p>
                        "
                    );
                }

                // ADMINS Y TECNICOS

                var destinatarios = _context.Usuarios
                    .Where(x =>
                        x.Rol == "Administrador"
                        || x.Rol == "Tecnico"
                    )
                    .ToList();

                foreach (var destino in destinatarios)
                {
                    if (!string.IsNullOrEmpty(destino.Correo))
                    {
                        _correoService.EnviarCorreo(
                            destino.Correo,
                            $"Nuevo Ticket Registrado - {ticket.Codigo}",
                            $@"
                            <h2>Nuevo Ticket Registrado</h2>

                            <p><b>Código:</b> {ticket.Codigo}</p>
                            <p><b>Usuario:</b> {usuario.Nombre}</p>
                            <p><b>Título:</b> {ticket.Titulo}</p>
                            <p><b>Descripción:</b> {ticket.Descripcion}</p>
                            <p><b>Prioridad:</b> {ticket.Prioridad}</p>
                            <p><b>Tipo:</b> {ticket.Tipo}</p>
                            <p><b>Grupo:</b> {ticket.Grupo}</p>
                            "
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return Ok(ticket);
        }

        // =========================================
        // EDITAR TICKET
        // =========================================

       
[HttpPut("{id}")]
public async Task<IActionResult> Editar(
    int id,
    [FromBody] EditarTicketDTO dto)
        {
            var ticket = _context.Tickets
                .FirstOrDefault(x => x.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            // COPIA ANTES

            var ticketAnterior = new Ticket
            {
                Descripcion = ticket.Descripcion
            };

            // CAMBIO

            ticket.Descripcion = dto.Descripcion;

            _context.SaveChanges();

            // AUDITORÍA

            await _auditoriaService.RegistrarCambios(
                "Tickets",
                ticket.Id,
                ticketAnterior,
                ticket
            );

            return Ok(ticket);
        }



        // =========================================
        // CAMBIAR ESTADO
        // =========================================

       
[Authorize(Roles = "Administrador,Tecnico")]
[HttpPut("estado/{id}")]
public async Task<IActionResult> CambiarEstado(
    int id,
    [FromBody] string estado)
        {
            var ticket = _context.Tickets
                .FirstOrDefault(x => x.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            // COPIA ANTES

            var ticketAnterior = new Ticket
            {
                Estado = ticket.Estado,
                FechaCierre = ticket.FechaCierre
            };

            // CAMBIOS

            ticket.Estado = estado;

            if (estado == "Cerrado")
            {
                ticket.FechaCierre = DateTime.Now;
            }

            _context.SaveChanges();

            // AUDITORÍA AUTOMÁTICA

            await _auditoriaService.RegistrarCambios(
                "Tickets",
                ticket.Id,
                ticketAnterior,
                ticket
            );

            return Ok(ticket);
        }



        // =========================================
        // ACTUALIZAR TIPO Y GRUPO
        // =========================================

        [Authorize(Roles = "Administrador,Tecnico")]
        [HttpPut("clasificacion/{id}")]
        public IActionResult ActualizarClasificacion(
            int id,
            [FromBody] ClasificacionTicketDTO dto)
        {
            var ticket = _context.Tickets
                .FirstOrDefault(x => x.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            ticket.Tipo = dto.Tipo;

            ticket.Grupo = dto.Grupo;

            _context.SaveChanges();

            return Ok(ticket);
        }

        // =========================================
        // SUBIR ARCHIVOS
        // =========================================

        [HttpPost("upload/{ticketId}")]
        public async Task<IActionResult> SubirArchivo(
            int ticketId,
            List<IFormFile> archivos
        )
        {
            var ticket = _context.Tickets
                .FirstOrDefault(x => x.Id == ticketId);

            if (ticket == null)
            {
                return NotFound("Ticket no encontrado");
            }

            if (archivos == null || archivos.Count == 0)
            {
                return BadRequest("No se enviaron archivos");
            }

            var carpetaUploads = Path.Combine(
                _environment.WebRootPath,
                 "Uploads"
            );

            if (!Directory.Exists(carpetaUploads))
            {
                Directory.CreateDirectory(carpetaUploads);
            }

            foreach (var archivo in archivos)
            {
                var nombreArchivo =
                    Guid.NewGuid().ToString()
                    + Path.GetExtension(archivo.FileName);

                var rutaCompleta = Path.Combine(
                    carpetaUploads,
                    nombreArchivo
                );

                using (var stream = new FileStream(
                    rutaCompleta,
                    FileMode.Create
                ))
                {
                    await archivo.CopyToAsync(stream);
                }

                var nuevoArchivo = new ArchivoTicket
                {
                    TicketId = ticketId,

                    NombreArchivo = archivo.FileName,

                    RutaArchivo = "/Uploads/" + nombreArchivo,

                    Fecha = DateTime.Now
                };

                _context.ArchivosTicket.Add(nuevoArchivo);
            }

            await _context.SaveChangesAsync();

            return Ok();
        }
         // descargar archivos//
        [AllowAnonymous]
        [HttpGet("descargar/{id}")]
        public async Task<IActionResult> DescargarArchivo(int id)
        {
            var archivo = await _context.ArchivosTicket
                .FirstOrDefaultAsync(a => a.Id == id);

            if (archivo == null)
            {
                return NotFound();
            }

            var rutaCompleta = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                archivo.RutaArchivo.TrimStart('/')
            );

            if (!System.IO.File.Exists(rutaCompleta))
            {
                return NotFound("Archivo no encontrado");
            }

            var bytes = await System.IO.File.ReadAllBytesAsync(rutaCompleta);

            return File(
                bytes,
                "application/octet-stream",
                archivo.NombreArchivo
            );
        }

        // =========================================
        // ASIGNAR TECNICO
        // =========================================

        
[Authorize(Roles = "Administrador,Tecnico")]
[HttpPut("asignar/{id}")]
public async Task<IActionResult> AsignarTecnico(
    int id,
    [FromBody] int tecnicoId)
        {
            var ticket = _context.Tickets
                .FirstOrDefault(x => x.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            // COPIA ANTES

            var ticketAnterior = new Ticket
            {
                TecnicoId = ticket.TecnicoId
            };

            // CAMBIO

            ticket.TecnicoId = tecnicoId;

            _context.SaveChanges();

            // AUDITORÍA

            await _auditoriaService.RegistrarCambios(
                "Tickets",
                ticket.Id,
                ticketAnterior,
                ticket
            );

            return Ok(ticket);
        }


    }
}