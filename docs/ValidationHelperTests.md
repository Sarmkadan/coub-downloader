# ValidationHelperTests

Unit tests for the `ValidationHelper` class, providing validation and sanitization utilities for common data types and formats used in the coub-downloader project. The tests cover email validation, URL parsing (including Coub-specific URLs), bitrate and resolution constraints, frame rate validation, file name sanitization, and a flexible validation builder pattern for composing multiple validation rules.

## API

### `public void IsValidEmail_VariousInputs_ReturnsExpectedResult`

Tests the `ValidationHelper.IsValidEmail` method with various email formats. Validates that the method correctly identifies valid and invalid email addresses according to standard conventions.

- **Parameters**: None
- **Return value**: None (asserts expected results)
- **Throws**: Not applicable

---

### `public void IsValidUrl_VariousSchemes_ReturnsExpectedResult`

Tests the `ValidationHelper.IsValidUrl` method across different URL schemes (e.g., `http`, `https`, `ftp`). Ensures the method accurately validates URLs with supported schemes.

- **Parameters**: None
- **Return value**: None (asserts expected results)
- **Throws**: Not applicable

---

### `public void IsValidCoubUrl_WithViewPath_ReturnsTrue`

Tests that `ValidationHelper.IsValidCoubUrl` correctly identifies valid Coub URLs containing the `/view/` path segment. Confirms the method recognizes Coub-specific URL patterns.

- **Parameters**: None
- **Return value**: None (asserts expected results)
- **Throws**: Not applicable

---
### `public void IsValidCoubUrl_NonCoubDomain_ReturnsFalse`

Tests that `ValidationHelper.IsValidCoubUrl` returns `false` for URLs that do not originate from a Coub domain (e.g., `example.com/view/abc123`). Validates domain filtering logic.

- **Parameters**: None
- **Return value**: None (asserts expected results)
- **Throws**: Not applicable

---
### `public void IsValidBitrate_BoundaryValues_ReturnsExpectedResult`

Tests `ValidationHelper.IsValidBitrate` with boundary values (e.g., minimum, maximum, and values just outside the valid range). Ensures the method enforces bitrate constraints correctly.

- **Parameters**: None
- **Return value**: None (asserts expected results)
- **Throws**: Not applicable

---
### `public void IsValidResolution_StandardHD_ReturnsTrue`

Tests that `ValidationHelper.IsValidResolution` accepts standard high-definition resolutions (e.g., 1280×720, 1920×1080). Validates common HD format support.

- **Parameters**: None
- **Return value**: None (asserts expected results)
- **Throws**: Not applicable

---
### `public void IsValidResolution_ZeroWidth_ReturnsFalse`

Tests that `ValidationHelper.IsValidResolution` rejects resolutions with zero or negative width or height. Confirms the method enforces non-zero dimension constraints.

- **Parameters**: None
- **Return value**: None (asserts expected results)
- **Throws**: Not applicable

---
### `public void IsValidResolution_ExceedsMaxDimension_ReturnsFalse`

Tests that `ValidationHelper.IsValidResolution` rejects resolutions exceeding the maximum allowed dimension (e.g., 8192×8192). Validates upper-bound enforcement.

- **Parameters**: None
- **Return value**: None (asserts expected results)
- **Throws**: Not applicable

---
### `public void IsValidFrameRate_BoundaryValues_ReturnsExpectedResult`

Tests `ValidationHelper.IsValidFrameRate` with boundary values (e.g., minimum, maximum, and values just outside the valid range). Ensures the method enforces frame rate constraints correctly.

- **Parameters**: None
- **Return value**: None (asserts expected results)
- **Throws**: Not applicable

---
### `public void SanitizeFileName_ContainsInvalidChars_RemovesThem`

Tests that `ValidationHelper.SanitizeFileName` removes or replaces characters that are invalid in file names (e.g., `\`, `/`, `:`, `*`, `?`, `"`, `<`, `>`, `|`). Validates sanitization logic.

- **Parameters**: None
- **Return value**: None (asserts expected results)
- **Throws**: Not applicable

---
### `public void SanitizeFileName_AlreadyClean_ReturnsUnchanged`

Tests that `ValidationHelper.SanitizeFileName` returns the input unchanged when it contains no invalid characters. Validates idempotent behavior.

- **Parameters**: None
- **Return value**: None (asserts expected results)
- **Throws**: Not applicable

---
### `public void ValidationBuilder_AllRulesPassed_IsValidTrue`

Tests the `ValidationHelper.ValidationBuilder` fluent interface when all validation rules pass. Confirms that the builder returns `true` and collects no errors.

- **Parameters**: None
- **Return value**: None (asserts expected results)
- **Throws**: Not applicable

---
### `public void ValidationBuilder_EmptyRequiredField_CollectsError`

Tests the `ValidationHelper.ValidationBuilder` when a required field is empty. Validates that the builder collects the appropriate error message.

- **Parameters**: None
- **Return value**: None (asserts expected results)
- **Throws**: Not applicable

---
### `public void ValidationBuilder_OutOfRangeValue_CollectsError`

Tests the `ValidationHelper.ValidationBuilder` when a numeric value is out of the allowed range. Validates that the builder collects the appropriate error message.

- **Parameters**: None
- **Return value**: None (asserts expected results)
- **Throws**: Not applicable

---
### `public void ValidationBuilder_ThrowIfInvalid_ThrowsArgumentException`

Tests the `ValidationHelper.ValidationBuilder.ThrowIfInvalid` method. Validates that it throws an `ArgumentException` when validation fails, including the collected error messages.

- **Parameters**: None
- **Return value**: None (asserts expected exceptions)
- **Throws**: `ArgumentException` when validation fails

---

## Usage
