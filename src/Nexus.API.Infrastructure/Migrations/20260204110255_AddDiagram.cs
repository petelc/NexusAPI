using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.API.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDiagram : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "CodeSnippets",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LanguageName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FileExtension = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    LanguageVersion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LineCount = table.Column<int>(type: "int", nullable: false),
                    CharacterCount = table.Column<int>(type: "int", nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    ForkCount = table.Column<int>(type: "int", nullable: false),
                    ViewCount = table.Column<int>(type: "int", nullable: false),
                    OriginalSnippetId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeSnippets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Diagrams",
                schema: "dbo",
                columns: table => new
                {
                    DiagramId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DiagramType = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CanvasWidth = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CanvasHeight = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    BackgroundColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    GridSize = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Diagrams", x => x.DiagramId);
                });

            migrationBuilder.CreateTable(
                name: "CodeSnippetTags",
                schema: "dbo",
                columns: table => new
                {
                    CodeSnippetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TagId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeSnippetTags", x => new { x.CodeSnippetId, x.TagId });
                    table.ForeignKey(
                        name: "FK_CodeSnippetTags_CodeSnippets_CodeSnippetId",
                        column: x => x.CodeSnippetId,
                        principalSchema: "dbo",
                        principalTable: "CodeSnippets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CodeSnippetTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SnippetForks",
                schema: "dbo",
                columns: table => new
                {
                    OriginalSnippetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ForkedSnippetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ForkedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ForkedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SnippetForks", x => new { x.OriginalSnippetId, x.ForkedSnippetId });
                    table.ForeignKey(
                        name: "FK_SnippetForks_CodeSnippets_OriginalSnippetId",
                        column: x => x.OriginalSnippetId,
                        principalSchema: "dbo",
                        principalTable: "CodeSnippets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiagramConnections",
                schema: "dbo",
                columns: table => new
                {
                    ConnectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceElementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetElementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConnectionType = table.Column<int>(type: "int", nullable: false),
                    StrokeColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    StrokeWidth = table.Column<int>(type: "int", nullable: false),
                    StrokeDashArray = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ControlPoints = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiagramId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiagramConnections", x => x.ConnectionId);
                    table.CheckConstraint("CK_DiagramConnections_Elements", "[SourceElementId] <> [TargetElementId]");
                    table.CheckConstraint("CK_DiagramConnections_Type", "[ConnectionType] BETWEEN 0 AND 3");
                    table.ForeignKey(
                        name: "FK_DiagramConnections_Diagrams_DiagramId",
                        column: x => x.DiagramId,
                        principalSchema: "dbo",
                        principalTable: "Diagrams",
                        principalColumn: "DiagramId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiagramElements",
                schema: "dbo",
                columns: table => new
                {
                    ElementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShapeType = table.Column<int>(type: "int", nullable: false),
                    PositionX = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PositionY = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Width = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Height = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    FillColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    StrokeColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    StrokeWidth = table.Column<int>(type: "int", nullable: false),
                    FontSize = table.Column<int>(type: "int", nullable: false),
                    FontFamily = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Opacity = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    Rotation = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ZIndex = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CustomProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiagramId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiagramElements", x => x.ElementId);
                    table.CheckConstraint("CK_DiagramElements_Opacity", "[Opacity] >= 0 AND [Opacity] <= 1");
                    table.CheckConstraint("CK_DiagramElements_Rotation", "[Rotation] >= 0 AND [Rotation] < 360");
                    table.CheckConstraint("CK_DiagramElements_ShapeType", "[ShapeType] BETWEEN 0 AND 99");
                    table.CheckConstraint("CK_DiagramElements_Size", "[Width] > 0 AND [Height] > 0");
                    table.ForeignKey(
                        name: "FK_DiagramElements_Diagrams_DiagramId",
                        column: x => x.DiagramId,
                        principalSchema: "dbo",
                        principalTable: "Diagrams",
                        principalColumn: "DiagramId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiagramLayers",
                schema: "dbo",
                columns: table => new
                {
                    LayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DiagramId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiagramLayers", x => x.LayerId);
                    table.CheckConstraint("CK_DiagramLayers_Order", "[Order] >= 0");
                    table.ForeignKey(
                        name: "FK_DiagramLayers_Diagrams_DiagramId",
                        column: x => x.DiagramId,
                        principalSchema: "dbo",
                        principalTable: "Diagrams",
                        principalColumn: "DiagramId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CodeSnippets_CreatedAt",
                schema: "dbo",
                table: "CodeSnippets",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CodeSnippets_CreatedBy",
                schema: "dbo",
                table: "CodeSnippets",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CodeSnippets_OriginalSnippetId",
                schema: "dbo",
                table: "CodeSnippets",
                column: "OriginalSnippetId",
                filter: "[OriginalSnippetId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CodeSnippets_PublicDeleted",
                schema: "dbo",
                table: "CodeSnippets",
                column: "IsPublic");

            migrationBuilder.CreateIndex(
                name: "IX_CodeSnippetTags_TagId",
                schema: "dbo",
                table: "CodeSnippetTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_DiagramConnections_DiagramId",
                schema: "dbo",
                table: "DiagramConnections",
                column: "DiagramId");

            migrationBuilder.CreateIndex(
                name: "IX_DiagramConnections_SourceElement",
                schema: "dbo",
                table: "DiagramConnections",
                column: "SourceElementId");

            migrationBuilder.CreateIndex(
                name: "IX_DiagramConnections_TargetElement",
                schema: "dbo",
                table: "DiagramConnections",
                column: "TargetElementId");

            migrationBuilder.CreateIndex(
                name: "IX_DiagramElements_DiagramId",
                schema: "dbo",
                table: "DiagramElements",
                column: "DiagramId");

            migrationBuilder.CreateIndex(
                name: "IX_DiagramElements_DiagramId_ZIndex",
                schema: "dbo",
                table: "DiagramElements",
                columns: new[] { "DiagramId", "ZIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_DiagramElements_LayerId",
                schema: "dbo",
                table: "DiagramElements",
                column: "LayerId");

            migrationBuilder.CreateIndex(
                name: "IX_DiagramLayers_DiagramId",
                schema: "dbo",
                table: "DiagramLayers",
                column: "DiagramId");

            migrationBuilder.CreateIndex(
                name: "IX_DiagramLayers_DiagramId_Order",
                schema: "dbo",
                table: "DiagramLayers",
                columns: new[] { "DiagramId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Diagrams_CreatedAt",
                schema: "dbo",
                table: "Diagrams",
                column: "CreatedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Diagrams_CreatedBy",
                schema: "dbo",
                table: "Diagrams",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Diagrams_DiagramType",
                schema: "dbo",
                table: "Diagrams",
                column: "DiagramType",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Diagrams_IsDeleted",
                schema: "dbo",
                table: "Diagrams",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Diagrams_UpdatedAt",
                schema: "dbo",
                table: "Diagrams",
                column: "UpdatedAt",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CodeSnippetTags",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "DiagramConnections",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "DiagramElements",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "DiagramLayers",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "SnippetForks",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Diagrams",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CodeSnippets",
                schema: "dbo");
        }
    }
}
