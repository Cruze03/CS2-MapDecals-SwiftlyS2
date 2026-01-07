using FluentMigrator;

[Migration(2025122701)]
public class CreateMapDecalsTable : Migration
{
    public override void Up()
    {
        Create.Table("cc_mapdecals")
            .WithColumn("id")
                .AsInt64()
                .PrimaryKey()
                .Identity()   // AUTO_INCREMENT

            .WithColumn("map")
                .AsString(64)
                .NotNullable()

            .WithColumn("decal_id")
                .AsString(64)
                .NotNullable()

            .WithColumn("decal_name")
                .AsString(64)
                .NotNullable()

            .WithColumn("position").AsString(64).NotNullable()
            .WithColumn("angles").AsString(64).NotNullable()

            .WithColumn("depth")
                .AsInt64()
                .NotNullable()
                .WithDefaultValue(12)

            .WithColumn("width")
                .AsFloat()
                .NotNullable()
                .WithDefaultValue(128)

            .WithColumn("height")
                .AsFloat()
                .NotNullable()
                .WithDefaultValue(128)

            .WithColumn("force_on_vip")
                .AsBoolean()
                .NotNullable()
                .WithDefaultValue(false)

            .WithColumn("is_active")
                .AsBoolean()
                .NotNullable()
                .WithDefaultValue(true);

        Create.Index("idx_map_decals_map")
            .OnTable("cc_mapdecals")
            .OnColumn("map").Ascending();
    }

    public override void Down()
    {
        Delete.Table("cc_mapdecals");
    }
}
