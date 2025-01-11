using System.IO.Compression;
using System.Text;

namespace Santa_Gifts_API.Tools;
/// <summary>
/// This class provides methods for unzipping and decompressing the the Amazon csv dataset as a string.
/// </summary>
public static class Dataset {
	/// <summary>
	/// Extracts the Amazon dataset from a zip file in the repository root, and decompresses it
	/// into a string.
	/// </summary>
	/// <returns>The contents of the decompressed file as a string.</returns>
	public static string Load() {
		string gzipFile = $"./data/products.csv.gz";
		return DecompressGzipFileToString(gzipFile);
	}

	/// <summary>
	/// Reads the contents of a gzipped file and returns it as a string.
	/// </summary>
	/// <param name="gzipFilePath">The path to the gzipped file.</param>
	/// <returns>The contents of the gzipped file as a string.</returns>
	private static string DecompressGzipFileToString(string gzipFilePath) {
		FileStream compressedFileStream = new(gzipFilePath, FileMode.Open, FileAccess.Read);
		GZipStream decompressionStream = new(compressedFileStream, CompressionMode.Decompress);
		MemoryStream memoryStream = new();

		decompressionStream.CopyTo(memoryStream);
		
		return Encoding.UTF8.GetString(memoryStream.ToArray());
	}
}
