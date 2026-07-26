using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Admission.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialAdmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "applicants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthId = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Gender = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    Citizenship = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    LastModifiedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_applicants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "education_level_combinations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstLevelId = table.Column<Guid>(type: "uuid", nullable: false),
                    SecondLevelId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsAllowed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_education_level_combinations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "education_levels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OneCId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_education_levels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "faculties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OneCId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_faculties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InboxState",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsumerId = table.Column<Guid>(type: "uuid", nullable: false),
                    LockId = table.Column<Guid>(type: "uuid", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    Received = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReceiveCount = table.Column<int>(type: "integer", nullable: false),
                    ExpirationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Consumed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Delivered = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSequenceNumber = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxState", x => x.Id);
                    table.UniqueConstraint("AK_InboxState_MessageId_ConsumerId", x => new { x.MessageId, x.ConsumerId });
                });

            migrationBuilder.CreateTable(
                name: "OutboxState",
                columns: table => new
                {
                    OutboxId = table.Column<Guid>(type: "uuid", nullable: false),
                    LockId = table.Column<Guid>(type: "uuid", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Delivered = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSequenceNumber = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxState", x => x.OutboxId);
                });

            migrationBuilder.CreateTable(
                name: "education_document_types",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OneCId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    CurrentEducationLevelId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_education_document_types", x => x.Id);
                    table.ForeignKey(
                        name: "FK_education_document_types_education_levels_CurrentEducationL~",
                        column: x => x.CurrentEducationLevelId,
                        principalTable: "education_levels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "education_programs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OneCId = table.Column<Guid>(type: "uuid", nullable: false),
                    FacultyId = table.Column<Guid>(type: "uuid", nullable: false),
                    EducationLevelId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Language = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EducationForm = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_education_programs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_education_programs_education_levels_EducationLevelId",
                        column: x => x.EducationLevelId,
                        principalTable: "education_levels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_education_programs_faculties_FacultyId",
                        column: x => x.FacultyId,
                        principalTable: "faculties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "managers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthId = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FacultyId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_managers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_managers_faculties_FacultyId",
                        column: x => x.FacultyId,
                        principalTable: "faculties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessage",
                columns: table => new
                {
                    SequenceNumber = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EnqueueTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SentTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Headers = table.Column<string>(type: "text", nullable: true),
                    Properties = table.Column<string>(type: "text", nullable: true),
                    InboxMessageId = table.Column<Guid>(type: "uuid", nullable: true),
                    InboxConsumerId = table.Column<Guid>(type: "uuid", nullable: true),
                    OutboxId = table.Column<Guid>(type: "uuid", nullable: true),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    MessageType = table.Column<string>(type: "text", nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: true),
                    InitiatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DestinationAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ResponseAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    FaultAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ExpirationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessage", x => x.SequenceNumber);
                    table.ForeignKey(
                        name: "FK_OutboxMessage_InboxState_InboxMessageId_InboxConsumerId",
                        columns: x => new { x.InboxMessageId, x.InboxConsumerId },
                        principalTable: "InboxState",
                        principalColumns: new[] { "MessageId", "ConsumerId" });
                    table.ForeignKey(
                        name: "FK_OutboxMessage_OutboxState_OutboxId",
                        column: x => x.OutboxId,
                        principalTable: "OutboxState",
                        principalColumn: "OutboxId");
                });

            migrationBuilder.CreateTable(
                name: "applicant_documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    document_kind = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    DocumentTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Series = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    PlaceOfBirth = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IssuedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IssueDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_applicant_documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_applicant_documents_applicants_ApplicantId",
                        column: x => x.ApplicantId,
                        principalTable: "applicants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_applicant_documents_education_document_types_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalTable: "education_document_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "education_document_type_next_levels",
                columns: table => new
                {
                    ApplicableDocumentTypesId = table.Column<Guid>(type: "uuid", nullable: false),
                    NextEducationLevelsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_education_document_type_next_levels", x => new { x.ApplicableDocumentTypesId, x.NextEducationLevelsId });
                    table.ForeignKey(
                        name: "FK_education_document_type_next_levels_education_document_type~",
                        column: x => x.ApplicableDocumentTypesId,
                        principalTable: "education_document_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_education_document_type_next_levels_education_levels_NextEd~",
                        column: x => x.NextEducationLevelsId,
                        principalTable: "education_levels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "applicant_admissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ManagerId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_applicant_admissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_applicant_admissions_applicants_ApplicantId",
                        column: x => x.ApplicantId,
                        principalTable: "applicants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_applicant_admissions_managers_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "managers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "admission_programs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicantAdmissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    EducationProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    Priority = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admission_programs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_admission_programs_applicant_admissions_ApplicantAdmissionId",
                        column: x => x.ApplicantAdmissionId,
                        principalTable: "applicant_admissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_admission_programs_education_programs_EducationProgramId",
                        column: x => x.EducationProgramId,
                        principalTable: "education_programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_admission_programs_ApplicantAdmissionId_EducationProgramId",
                table: "admission_programs",
                columns: new[] { "ApplicantAdmissionId", "EducationProgramId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_admission_programs_EducationProgramId",
                table: "admission_programs",
                column: "EducationProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_applicant_admissions_ApplicantId",
                table: "applicant_admissions",
                column: "ApplicantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_applicant_admissions_ManagerId",
                table: "applicant_admissions",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_applicant_admissions_Status",
                table: "applicant_admissions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_applicant_documents_ApplicantId",
                table: "applicant_documents",
                column: "ApplicantId");

            migrationBuilder.CreateIndex(
                name: "IX_applicant_documents_DocumentTypeId",
                table: "applicant_documents",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_applicants_AuthId",
                table: "applicants",
                column: "AuthId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_education_document_type_next_levels_NextEducationLevelsId",
                table: "education_document_type_next_levels",
                column: "NextEducationLevelsId");

            migrationBuilder.CreateIndex(
                name: "IX_education_document_types_CurrentEducationLevelId",
                table: "education_document_types",
                column: "CurrentEducationLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_education_document_types_OneCId",
                table: "education_document_types",
                column: "OneCId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_education_level_combinations_FirstLevelId_SecondLevelId",
                table: "education_level_combinations",
                columns: new[] { "FirstLevelId", "SecondLevelId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_education_levels_OneCId",
                table: "education_levels",
                column: "OneCId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_education_programs_EducationLevelId",
                table: "education_programs",
                column: "EducationLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_education_programs_FacultyId",
                table: "education_programs",
                column: "FacultyId");

            migrationBuilder.CreateIndex(
                name: "IX_education_programs_OneCId",
                table: "education_programs",
                column: "OneCId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_faculties_OneCId",
                table: "faculties",
                column: "OneCId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InboxState_Delivered",
                table: "InboxState",
                column: "Delivered");

            migrationBuilder.CreateIndex(
                name: "IX_managers_AuthId",
                table: "managers",
                column: "AuthId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_managers_FacultyId",
                table: "managers",
                column: "FacultyId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_EnqueueTime",
                table: "OutboxMessage",
                column: "EnqueueTime");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_ExpirationTime",
                table: "OutboxMessage",
                column: "ExpirationTime");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_InboxMessageId_InboxConsumerId_SequenceNumber",
                table: "OutboxMessage",
                columns: new[] { "InboxMessageId", "InboxConsumerId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_OutboxId_SequenceNumber",
                table: "OutboxMessage",
                columns: new[] { "OutboxId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxState_Created",
                table: "OutboxState",
                column: "Created");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admission_programs");

            migrationBuilder.DropTable(
                name: "applicant_documents");

            migrationBuilder.DropTable(
                name: "education_document_type_next_levels");

            migrationBuilder.DropTable(
                name: "education_level_combinations");

            migrationBuilder.DropTable(
                name: "OutboxMessage");

            migrationBuilder.DropTable(
                name: "applicant_admissions");

            migrationBuilder.DropTable(
                name: "education_programs");

            migrationBuilder.DropTable(
                name: "education_document_types");

            migrationBuilder.DropTable(
                name: "InboxState");

            migrationBuilder.DropTable(
                name: "OutboxState");

            migrationBuilder.DropTable(
                name: "applicants");

            migrationBuilder.DropTable(
                name: "managers");

            migrationBuilder.DropTable(
                name: "education_levels");

            migrationBuilder.DropTable(
                name: "faculties");
        }
    }
}
