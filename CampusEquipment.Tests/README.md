# Campus Equipment tests

Run from the solution directory:

```sh
dotnet test CampusEquipment.Tests/CampusEquipment.Tests.csproj
```

The xUnit/Moq unit tests need no database. Each test gets isolated mocks.

| Submission test | Automated coverage |
| --- | --- |
| 1. Display equipment | Service mapping, MVC view model, API response and filter forwarding |
| 2. Create equipment | Service writes the supplied fields; MVC redirects; API returns 201 and a details location |
| 3. Duplicate PC-001 | Real service rejects duplicates and never inserts; both controllers handle/propagate rejection |
| 4. Invalid department | Real service rejects a nonexistent positive department ID without inserting; both controllers handle/propagate rejection |
| 5. Retired assignment | Real service rejects assignment without changing status or writing; controller rejection handling |
| 6. Maintenance assignment | Same checks for UnderMaintenance; Available-to-Assigned is also tested as an allowed transition |
| 7. API/Swagger | API controller and exception-handler unit tests; Swagger UI and HTTP pipeline require separate integration/manual checks |
| 8. SQL Server verification | Optional API and MVC controller-to-database round-trip integration tests |

## SQL Server tests

Set `CAMPUS_EQUIPMENT_TEST_CONNECTION` to a connection string for a **dedicated test database** whose name includes `test`. Provision the same database-first Department and Equipment schema as the application before running. These tests do not create a schema or use migrations. Supply credentials through the environment, not source control.

```sh
dotnet test CampusEquipment.Tests/CampusEquipment.Tests.csproj --filter Category=SqlServer
```

Without the environment variable, these two tests are explicitly skipped. They invoke real controllers, services, repositories and SQL Server, insert unique equipment and a department, read the row back with tracking cleared, and roll back their transaction in `finally`. SQL Server identity counters can still advance after rollback. These checks verify SQL writes within a transaction, not cross-connection visibility after commit.

## Boundaries and existing discrepancy

Controller unit tests do not execute Razor rendering, routing, model binding, automatic API validation, antiforgery filters or Swagger. MVC invalid-model tests explicitly populate ModelState. Service tests run real DTO validation. Exception-handler tests check status codes and JSON separately.

The current service throws ValidationException for duplicate codes and prohibited assignments. The API consequently returns 400. The requirements call for 409 for duplicate asset codes; the API already maps BusinessConflictException to 409, but the service does not currently throw it. The tests document current behavior without changing production code. A mock-based 409 handler test alone does not establish that duplicate requests return 409 end to end.

For the final submission, also perform the listed browser/Swagger operations against your test SQL Server instance and inspect the persisted rows. The unit suite does not claim to replace that manual acceptance check.
