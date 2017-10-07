
// external framework assemblies
#r "Newtonsoft.Json"

// namespace references
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Configuration;
using System.Xml;
using System.Data;
using System.Data.SqlClient;

//main function definition
public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    // writing to log for troubleshooting
    log.Info("Drew's example has been triggered.");    

    // use try-catch to add error handling
    try{
        //basic variables needed for the originating request
        var httpStatus = HttpStatusCode.OK;
        string responseString = "";

        // get request body information
        dynamic data = await req.Content.ReadAsAsync<object>();

        // setup objects to load notes
        alldocuments docs = new alldocuments();
        var notescollection = new List<xNote> ();

        // create connection to sql server
        var cnnString  = ConfigurationManager.ConnectionStrings["SqlConnection"].ConnectionString;
        using(var connection = new SqlConnection(cnnString)) {
            string sql = @"select TOP 50 N.NID, SUBSTRING(NOTE,1,1000) AS NOTE from JOBNOTES N
                            LEFT JOIN JOBNotes_Sentiment S ON N.NId = S.NId
                            WHERE S.NId IS NULL AND N.createdby = @PID
                            ORDER BY N.NID DESC";
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                // open the connection and set the parameter
                connection.Open();
                var aParam = new SqlParameter("PID", SqlDbType.VarChar);
                aParam.Value = data?.pid;
                command.Parameters.Add(aParam);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                     if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            // for each row in the resultset
                            // create a new xnote object
                            // set its attributes from the results
                            // add the xnote to the collection
                            xNote jobnote = new xNote();
                            jobnote.text = reader.GetString(reader.GetOrdinal("Note"));
                            jobnote.id = reader.GetInt32(reader.GetOrdinal("NID"));
                            jobnote.language = "en";
                            notescollection.Add(jobnote);
                            //log.Info("hi");
                        }
                    }
                    reader.Close();

                }
                connection.Close();
            }

        }
        docs.documents = notescollection;
        // convert the object to a json string for the text API
        string xnotejson = JsonConvert.SerializeObject(docs);

        // create an text analytics api request
        var requestData = new StringContent(xnotejson);
        using(var client = new HttpClient()) {
            string Url = "https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/sentiment";
            client.BaseAddress = new Uri(Url);
            
            // add headers to the request
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept","application/json");
            //insert your cognitive services API key here
            client.DefaultRequestHeaders.TryAddWithoutValidation("Ocp-Apim-Subscription-Key","yourkeystring");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpContent body = requestData;
            body.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // send the request and check the response
            var result = await client.PostAsync(Url, body);
            string resultContent = await result.Content.ReadAsStringAsync();
            dynamic sentiments = JsonConvert.DeserializeObject(resultContent);
            if (sentiments.documents == null)
            {
                // return a bad request code to the original request
                return req.CreateResponse(HttpStatusCode.BadRequest, new
                    {
                        error = "Bad response from Azure text analysis."
                    });
            }

        
            // manipulate the response into an xml format
            XmlDocument xmldoc = JsonConvert.DeserializeXmlNode(resultContent,"documents");

            // create a dataset for sqlbulkcopy
            DataSet ds = new DataSet("JsonData");
            XmlReader xr = new XmlNodeReader(xmldoc);
            DataTable dt = new DataTable();
            ds.ReadXml(xr);
            dt = ds.Tables[0];

            // take json response from text api and put it in the original request response
            responseString = resultContent;

            // create a new sql connection to put the text api results into a database
            using (var connection = new SqlConnection(cnnString))
            {
                connection.Open();
                // instead of individual tsql commands utilize sqlbulkcopy for insert
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = "JobNotes_Sentiment";
                    bulkCopy.ColumnMappings.Add("score","sentiment");
                    bulkCopy.ColumnMappings.Add("id","NId");
                    bulkCopy.WriteToServer(dt);
                    bulkCopy.Close();
                }
                connection.Close();
            }
        }

        // return a response to the original request
        var notesResponse = new StringContent(responseString);
        return new HttpResponseMessage() { Content = notesResponse, StatusCode = httpStatus };
    } catch (Exception ex) {
        // if the function hit the catch block, log an error and return and error code
        log.Error($"C# Http trigger function exception: {ex.Message}");
        return new HttpResponseMessage() { Content = new StringContent(""), StatusCode = HttpStatusCode.InternalServerError };
    }

}


//internal objects for structuring information similar to the cognitive services json
internal class xNote
{
    internal string language;
    internal int id;
    internal string text;
}
internal class alldocuments
{
    internal List<xNote> documents;
}