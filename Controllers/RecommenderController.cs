using Microsoft.AspNetCore.Authorization;
using Santa_Gifts_API.AI.Services;
using Microsoft.AspNetCore.Mvc;
using Santa_Gifts_API.DTOs;

namespace Santa_Gifts_API.Controllers;
	[ApiController]
	[Route("API/[controller]")]
	public class RecommenderController(IRecommenderService _recommenderService) : ControllerBase {
		/// <summary>
		/// The recommender service used to generate gift recommendations based on a sentence.
		/// </summary>
		private readonly IRecommenderService Recommender = _recommenderService;

		/// <summary>
		/// Recommends a list of gift items based on a given sentence.
		/// </summary>
		/// <param name="sentence">The sentence to get gift item recommendations for.</param>
		/// <returns>A IActionResult with the Ok result containing a list of gift items, or a BadRequest result with an error message if the recommendation fails.</returns>
		[HttpPost]
		[Authorize]
		[ProducesResponseType(typeof(Recommendations), 200)]
		public IActionResult GetRecommendations([FromBody] string sentence) {
			try {
				Recommendations list = Recommender.Recommend(sentence);
				return Ok(list);
			}
			catch (Exception ex) {
				return BadRequest(new { error = ex.Message, stackTrace = ex.StackTrace });
			}
		}
	}
