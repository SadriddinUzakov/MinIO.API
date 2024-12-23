using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using minio.Domain.Enumerations;

namespace minio.Domain.entity
{
    [Table("dt_files")]
    public class FileEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Column("is_deleted", TypeName = "integer")]
        public bool Deleted { get; set; } = false;

        [MaxLength(500)]
        public string Name { get; set; }

        [MaxLength(25)]
        public string Extension { get; set; }

        public long Size { get; set; }
        public string ContentType { get; set; }
        public string Path { get; set; }

        [EnumDataType(typeof(SaveMode))]
        public SaveMode SaveMode { get; set; }

        public string Etag { get; set; }
        public string DbFile { get; set; }
        public string ObjectName { get; set; }
        public string BucketName { get; set; }
        public string UniqueKey { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        [Column("created_by_id")]
        public Guid? CreatedBy { get; set; }

        [Column("updated_by_id")]
        public Guid? UpdatedBy { get; set; }

        public string GetFileName()
        {
            return $"{Name}.{Extension}";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Extension, Size, ContentType);
        }
    }
}
