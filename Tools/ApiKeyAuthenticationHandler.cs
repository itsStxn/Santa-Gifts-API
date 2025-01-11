using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.Security.Claims;

namespace Santa_Gifts_API.Tools;
/// <summary>
/// Handles API key authentication by validating the API key provided in the request headers.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ApiKeyAuthenticationHandler"/> class.
/// </remarks>
/// <param name="options">The options for the authentication handler.</param>
/// <param name="logger">The logger to use for logging authentication events.</param>
/// <param name="encoder">The encoder to use for encoding/decoding strings in the authentication process.</param>
public class ApiKeyAuthenticationHandler(
	IOptionsMonitor<AuthenticationSchemeOptions> options,
	ILoggerFactory logger,
	UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder) {
	/// <summary>
	/// Gets or sets the name of the request header that contains the API key.
	/// </summary>
	private const string API_KEY_HEADER_NAME = "MY-API-KEY";

	/// <summary>
	/// Handles API key authentication.
	/// </summary>
	/// <returns>An <see cref="AuthenticateResult"/> representing the authentication outcome.</returns>
	protected override Task<AuthenticateResult> HandleAuthenticateAsync() {
		if (!Request.Headers.TryGetValue(API_KEY_HEADER_NAME, out var extractedApiKey)) {
			return Task.FromResult(AuthenticateResult.Fail("API Key was not provided."));
		}

		var apiKey = Environment.GetEnvironmentVariable("API_KEY");
		if (apiKey is null || !apiKey.Equals(extractedApiKey)) {
			return Task.FromResult(AuthenticateResult.Fail("Invalid API Key."));
		}

		var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "ApiKeyUser") };
		var identity = new ClaimsIdentity(claims, Scheme.Name);
		var principal = new ClaimsPrincipal(identity);
		var ticket = new AuthenticationTicket(principal, Scheme.Name);

		return Task.FromResult(AuthenticateResult.Success(ticket));
	}
}
