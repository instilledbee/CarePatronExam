# Carepatron Backend Engineer Exam

## Requirements that were implemented:
* Implemented the following endpoints:
  * `GET /clients` - retrieve a list of all clients saved
  * `POST /clients` - create a new client entity
  * `PUT /clients` - update an existing client using the given ID
  * `GET /clients/search/{query}` - search for clients whose first or last name matches the given query string (case insensitive)
* Model validation was implemented using `FluentValidation`
  * All client fields were implemented as required
  * Endpoints accepting a `client` object return 400 if the model state is invalid
* A "mock" event emitter publishes client creation events
  * Events are stored in an in-memory `List`
  * Logs are written for client creation events
 
 ## Additional items implemented:
 * Endpoint for getting a specific client by ID:
   * `GET /clients/{id}` - returns 200 if found, or 404 if not found
   * As the create client endpoint returns the `Location` header where the resource was created, that value corresponds to the GEt endpoint where the caller can find the entity that was created
 * Integration tests for the API; please see the `api.Tests` project
 
 ## Nice-to-haves that could have been implemented, given more time:
 * Implement pagination for the get endpoint; for larger datasets this would be something to consider
 * More user-friendly responses for error scenarios - for example, updating a client that does not exist
 * Separate handler classes for each endpoint, ideally implemented using CQRS
   * Having the separate classes would also help unit test business logic
 * Better validation for client fields
   * For phone numbers, each country has unique number formats - one option was to integrate [libphonenumber-csharp](https://github.com/twcclegg/libphonenumber-csharp)
   * Validate if the email is in the correct format - FluentValidation's `EmailAddress()` validator only checks if the string contains an `@` for ASP.NET Core implementations. [Source](https://docs.fluentvalidation.net/en/latest/built-in-validators.html#email-validator)
   * For client IDs, given strings are accepted, use a known ID format, for example GUIDs.
 * Implement a simple MQ for the event emitter, and add a sample console app within the solution to show events being consumed
