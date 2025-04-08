using Azure;
using Azure.AI.ContentSafety;
using Microsoft.AspNetCore.Mvc;
using webapi.event_.Domains;
using webapi.event_.Interfaces;

namespace Event_.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ComentarioEventoController : ControllerBase
    {
        private readonly IComentariosEventosRepository _comentarioEventosRepository;
        private readonly ContentSafetyClient _contentSafetyClient;
        public ComentarioEventoController(ContentSafetyClient contentSafetyClient, IComentariosEventosRepository comentariosEventosRepository)
        {
            _comentarioEventosRepository = comentariosEventosRepository;
            _contentSafetyClient = contentSafetyClient;
        }

        [HttpPost]
        public async Task<IActionResult> Post(ComentariosEventos comentario)
        {
            try
            {
                if (string.IsNullOrEmpty(comentario.Descricao))
                {
                    return BadRequest("O texto a ser moderado não pode estar vazio!");
                }
                //criar objeto de analise do content safety
                var request = new AnalyzeTextOptions(comentario.Descricao);

                //chamar a API do Content Safety
                Response<AnalyzeTextResult> response = await _contentSafetyClient.AnalyzeTextAsync(request);
                //Await aguarde o anterior me responder pra ter seu retorno

                //verificar se texto analisado tem alguma severidade
                bool temConteudoImproprio = response.Value.CategoriesAnalysis.Any(c => c.Severity > 0);

                comentario.Exibe = !temConteudoImproprio;

                _comentarioEventosRepository.Cadastrar(comentario);

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

    }
}