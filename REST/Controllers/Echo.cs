using System.ComponentModel.DataAnnotations;
using System.Globalization;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

public class Rest : Controller
{
    private const byte Workers = 4;
    private static string Message { get; set; } = "";

    [HttpPut("echo")]
    public IActionResult PutEcho(string message)
    {
        Message = message;
        return Ok();
    }

    [HttpGet("echo")]
    public IActionResult GetEcho()
    {
        return Ok(Message);
    }

    [HttpPost("upload")]
    public IActionResult UploadFile([Required] IFormFile file)
    {
        if (Path.GetExtension(file.FileName).ToLower() != ".csv" || file.ContentType != "text/csv")
            return BadRequest("File must be csv");

        using var reader = new StreamReader(file.OpenReadStream());
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var records = csv.GetRecords<dynamic>().ToArray();
        var count = records.Length;
        /// Simulate long work set delay 5000
        Task.Run(() => Task.Delay(0)).ContinueWith(_ => records.Chunk((int)Math.Ceiling((double)count / Workers))
            .Select((objects, i) => (objects, i))
            .AsParallel()
            .ForAll(tuple => SaveInFile(csv.HeaderRecord, tuple.objects, tuple.i)));
        ;
        return Ok();
    }

    [HttpGet("top")]
    public IActionResult Top([Required] string field, [Required] [Range(0, int.MaxValue)] int count)
    {
        var query = Enumerable.Range(0, Workers).AsParallel()
            .SelectMany(i => ReadFile(i, field, count));
        try
        {
            var res = query.OrderByDescending(o =>
            {
                if (!o.TryGetValue(field, out var val)) return null;

                if (double.TryParse(val.ToString(), out var value)) return value;

                return o[field];
            }).Take(count).ToArray();
            return Ok(res);
        }
        catch (AggregateException e) when (e.InnerException is KeyNotFoundException)
        {
            return NotFound("field not exist");
        }
    }

    public void SaveInFile(string[] headers, dynamic[] rows, int worker)
    {
        using var writer = new StreamWriter($"DB/{worker}.csv");
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(rows);
    }

    public IEnumerable<IDictionary<string, object>> ReadFile(int worker, string sort_field, int count)
    {
        using var reader = new StreamReader($"DB/{worker}.csv");
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var records = csv.GetRecords<dynamic>().ToArray();

        var res = records.Cast<IDictionary<string, object>>().OrderByDescending(dict =>
        {
            if (double.TryParse(dict[sort_field].ToString(), out var value)) return value;

            return dict[sort_field];
        }).Take(count);
        return res;
    }
}