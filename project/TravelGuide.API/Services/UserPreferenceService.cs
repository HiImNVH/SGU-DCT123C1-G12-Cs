using TravelGuide.API.Repositories;
using TravelGuide.Core.Constants;

namespace TravelGuide.API.Services;

public interface IUserPreferenceService
{
    Task<bool> UpdateLanguageAsync(Guid userId, string langCode);
}

public class UserPreferenceService : IUserPreferenceService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserPreferenceService> _logger;

    public UserPreferenceService(IUserRepository userRepository, ILogger<UserPreferenceService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>
    /// Cap nhat ngon ngu ua thich, luu vao DB
    /// </summary>
    public async Task<bool> UpdateLanguageAsync(Guid userId, string langCode)
    {
        _logger.LogInformation("[info] - Cap nhat ngon ngu userId={UserId}, lang={Lang}", userId, langCode);

        if (!LanguageConstants.IsSupported(langCode))
        {
            _logger.LogWarning("[warn] - Ngon ngu khong hop le: {Lang}", langCode);
            return false;
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("[warn] - Khong tim thay user id={UserId}", userId);
            return false;
        }

        await _userRepository.UpdateLanguageAsync(userId, langCode);
        _logger.LogInformation("[info] - Da cap nhat ngon ngu userId={UserId} thanh {Lang}", userId, langCode);

        return true;
    }
}