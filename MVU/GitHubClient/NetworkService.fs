namespace GitHubClient

open System.Net
open System.Net.Http

open System.Text.Json
open GitHubClient.Models

module URlConstants =

    [<Literal>]
    let githubBaseUrl = "https://api.github.com/users/"

type GitHubError =
    | Non200Response

module GitHubService =

    let fetchWitHeader (urlString: string) =
        let client = new HttpClient()
        client.DefaultRequestHeaders.Add("User-Agent", "F# App")
        client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json")
        client.GetAsync(urlString)

    let getFollowers (searchTerm: string, page: int) =
        let urlString =
            $"{URlConstants.githubBaseUrl}%s{searchTerm}/followers?per_page=100&page=%d{page}"

        task {
            use! response = fetchWitHeader urlString
            response.EnsureSuccessStatusCode |> ignore

            let! followers = response.Content.ReadAsStringAsync() |> Async.AwaitTask
            let deserialized = JsonSerializer.Deserialize<Follower array>(followers)

            return
                match response.StatusCode with
                | HttpStatusCode.OK -> Ok deserialized
                | _ -> Error Non200Response
        }

    let getUserInfo (userName: string) =
        let urlString = $"https://api.github.com/users/%s{userName}"

        task {
            use! response = fetchWitHeader urlString
            response.EnsureSuccessStatusCode |> ignore
            let! followers = response.Content.ReadAsStringAsync() |> Async.AwaitTask
            let deserialized = JsonSerializer.Deserialize<User>(followers)

            return
                match response.StatusCode with
                | HttpStatusCode.OK -> Ok deserialized
                | _ -> Error Non200Response
        }
