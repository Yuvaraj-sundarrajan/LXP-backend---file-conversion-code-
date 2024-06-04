﻿using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using LXP.Common.Entities;
using LXP.Common.ViewModels;
using LXP.Core.IServices;
using LXP.Data.IRepository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Spire.Doc;
using Spire.Presentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LXP.Core.Services
{
    public class MaterialServices : IMaterialServices
    {
        private readonly IMaterialRepository _materialRepository;
        private readonly ICourseTopicRepository _courseTopicRepository;
        private readonly IMaterialTypeRepository _materialTypeRepository;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _contextAccessor;
        private Mapper _courseMaterialMapper;



        public MaterialServices(IMaterialTypeRepository materialTypeRepository, IMaterialRepository materialRepository, ICourseTopicRepository courseTopicRepository, IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
        {
            _materialRepository = materialRepository;
            _courseTopicRepository = courseTopicRepository;
            _materialTypeRepository = materialTypeRepository;
            _environment = environment;
            _contextAccessor = httpContextAccessor;
            var _configCourseMaterial = new MapperConfiguration(cfg => cfg.CreateMap<Material, MaterialListViewModel>().ReverseMap());
            _courseMaterialMapper = new Mapper(_configCourseMaterial);


        }
        public async Task<MaterialListViewModel> AddMaterial(MaterialViewModel material)
        {
            Topic topic = await _courseTopicRepository.GetTopicByTopicId(Guid.Parse(material.TopicId));
            MaterialType materialType = _materialTypeRepository.GetMaterialTypeByMaterialTypeId(Guid.Parse(material.MaterialTypeId));
            bool isMaterialExists = await _materialRepository.AnyMaterialByMaterialNameAndTopic(material.Name, topic);
            if (!isMaterialExists)
            {
                // Generate a unique file name
                var uniqueFileName = $"{Guid.NewGuid()}_{material.Material.FileName}";

                // Save the image to a designated folder (e.g., wwwroot/images)
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "CourseMaterial"); // Use WebRootPath
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    material.Material.CopyTo(stream); // Use await
                }
                Material materialCreation = new Material()
                {
                    MaterialId = Guid.NewGuid(),
                    Name = material.Name,
                    MaterialType = materialType,

                    CreatedBy = material.CreatedBy,
                    CreatedAt = DateTime.Now,
                    FilePath = uniqueFileName,
                    IsActive = true,
                    IsAvailable = true,
                    Duration = material.Duration,
                    Topic = topic,
                    ModifiedAt = null,
                    ModifiedBy = null
                };
                await _materialRepository.AddMaterial(materialCreation);
                return _courseMaterialMapper.Map<Material, MaterialListViewModel>(materialCreation);
            }
            else
            {
                return null;
            }
        }
        public async Task<bool> SoftDeleteMaterial(string materialId)
        {

            Material material = await _materialRepository.GetMaterialByMaterialId(Guid.Parse(materialId));
            material.IsActive = false;
            bool isMaterialDeleted = await _materialRepository.UpdateMaterial(material) > 0 ? true : false;
            if (isMaterialDeleted)
            {
                return true;
            }
            return false;

        }

        public async Task<List<MaterialListViewModel>> GetAllMaterialDetailsByTopicAndType(string topicId, string materialTypeId)
        {
            Topic topic = await _courseTopicRepository.GetTopicByTopicId(Guid.Parse(topicId));
            MaterialType materialType = _materialTypeRepository.GetMaterialTypeByMaterialTypeId(Guid.Parse(materialTypeId));

            List<Material> material = _materialRepository.GetAllMaterialDetailsByTopicAndType(topic, materialType);

            List<MaterialListViewModel> materialLists = new List<MaterialListViewModel>();

            foreach (var item in material)
            {
                MaterialListViewModel materialList = new MaterialListViewModel()
                {
                    MaterialId = item.MaterialId,
                    TopicName = item.Topic.Name,
                    MaterialType = item.MaterialType.Type,
                    Name = item.Name,
                    FilePath = String.Format("{0}://{1}{2}/wwwroot/CourseMaterial/{3}",
                                             _contextAccessor.HttpContext.Request.Scheme,
                                             _contextAccessor.HttpContext.Request.Host,
                                             _contextAccessor.HttpContext.Request.PathBase,
                                             item.FilePath),
                    Duration = item.Duration,
                    IsActive = item.IsActive,
                    IsAvailable = item.IsAvailable,
                    CreatedAt = item.CreatedAt,
                    CreatedBy = item.CreatedBy,
                    ModifiedAt = item.ModifiedAt,
                    ModifiedBy = item.ModifiedBy





                };
                materialLists.Add(materialList);
            }
            return materialLists;
        }

        public async Task<MaterialListViewModel> GetMaterialByMaterialNameAndTopic(string materialName, string topicId)
        {
            Topic topic = await _courseTopicRepository.GetTopicByTopicId(Guid.Parse(topicId));
            Material material = await _materialRepository.GetMaterialByMaterialNameAndTopic(materialName, topic);
            MaterialListViewModel materialView = new MaterialListViewModel()
            {
                MaterialId = material.MaterialId,
                TopicName = material.Topic.Name,
                MaterialType = material.MaterialType.Type,
                Name = material.Name,
                FilePath = String.Format("{0}://{1}{2}/wwwroot/CourseMaterial/{3}",
                                             _contextAccessor.HttpContext.Request.Scheme,
                                             _contextAccessor.HttpContext.Request.Host,
                                             _contextAccessor.HttpContext.Request.PathBase,
                                             material.FilePath),
                Duration = material.Duration,
                IsActive = material.IsActive,
                IsAvailable = material.IsAvailable,
                CreatedAt = material.CreatedAt,
                ModifiedAt = material.ModifiedAt,
                ModifiedBy = material.ModifiedBy,
                CreatedBy = material.CreatedBy







            };
            return materialView;
        }

        public async Task<bool> UpdateMaterial(MaterialUpdateViewModel material)
        {

            Material existMaterial = await _materialRepository.GetMaterialByMaterialId(Guid.Parse(material.MaterialId));
            var uniqueFileName = $"{Guid.NewGuid()} _{material.Material.FileName}";
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "CourseMaterial");
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await material.Material.CopyToAsync(stream);
            }

            existMaterial.Name = material.Name;
            existMaterial.FilePath = uniqueFileName;
            existMaterial.ModifiedBy = material.ModifiedBy;
            bool isMaterialUpdated = await _materialRepository.UpdateMaterial(existMaterial) > 0 ? true : false;
            return isMaterialUpdated;
            //if (existingMaterial.MaterialType.MaterialTypeId != Guid.Parse(material.MaterialId))
            //{
            //    MaterialType materialType = _materialTypeRepository.GetMaterialTypeByMaterialTypeId(Guid.Parse(material.MaterialId));
            //    existingMaterial.MaterialType = materialType;
            //}
            //}






            //existingMaterial.ModifiedAt = DateTime.Now;
            //existingMaterial.Duration = material.Duration;
            //existingMaterial.IsActive = material.IsActive;
            //existingMaterial.IsAvailable = material.IsAvailable;



        }

        /* public async Task<MaterialListViewModel> GetMaterialDetailsByMaterialId(string materialId)
         {
             Material material = await _materialRepository.GetMaterialByMaterialId(Guid.Parse(materialId));
             string filePath = Path.Combine("CourseMaterial", material.FilePath);

             string fileExtension = Path.GetExtension(material.FilePath).ToLower();

             // Check if the file is a DOC/DOCX, PPT/PPTX, TXT or RTF and convert if necessary
             if (fileExtension == ".doc" || fileExtension == ".docx" || fileExtension == ".ppt" || fileExtension == ".pptx" || fileExtension == ".txt" || fileExtension == ".rtf")
             {
                 filePath = await ConvertToPdfAsync(filePath, _environment, _contextAccessor);
             }

             string fileUrl = $"{_contextAccessor.HttpContext.Request.Scheme}://{_contextAccessor.HttpContext.Request.Host}{_contextAccessor.HttpContext.Request.PathBase}/wwwroot{filePath}";

             MaterialListViewModel materialView = new MaterialListViewModel()
             {
                 MaterialId = material.MaterialId,
                 TopicName = material.Topic.Name,
                 MaterialType = material.MaterialType.Type,
                 Name = material.Name,
                 FilePath = fileUrl,
                 Duration = material.Duration,
                 IsActive = material.IsActive,
                 IsAvailable = material.IsAvailable,
                 CreatedAt = material.CreatedAt,
                 ModifiedAt = material.ModifiedAt,
                 ModifiedBy = material.ModifiedBy,
                 CreatedBy = material.CreatedBy
             };
             return materialView;
         }

         // Method to convert DOC/DOCX, PPT/PPTX, TXT, RTF to PDF
         public static async Task<string> ConvertToPdfAsync(string filePath, IWebHostEnvironment environment, IHttpContextAccessor contextAccessor)
         {
             string webRootPath = environment.WebRootPath;
             string fullFilePath = Path.Combine(webRootPath, filePath);

             if (!File.Exists(fullFilePath))
             {
                 throw new FileNotFoundException($"The file was not found: {fullFilePath}");
             }

             string fileExtension = Path.GetExtension(filePath).ToLower();
             string pdfFilePath = Path.ChangeExtension(fullFilePath, ".pdf");

             if (fileExtension == ".doc" || fileExtension == ".docx")
             {
                 Spire.Doc.Document document = new Spire.Doc.Document();
                 document.LoadFromFile(fullFilePath);
                 document.SaveToFile(pdfFilePath, Spire.Doc.FileFormat.PDF);
             }
             else if (fileExtension == ".ppt" || fileExtension == ".pptx")
             {
                 // Convert PPT/PPTX to PDF using FreeSpire.Presentation
                 Spire.Presentation.Presentation presentation = new Spire.Presentation.Presentation();
                 presentation.LoadFromFile(fullFilePath);
                 presentation.SaveToFile(pdfFilePath, Spire.Presentation.FileFormat.PDF);
             }
             else if (fileExtension == ".txt" || fileExtension == ".rtf")
             {
                 // Convert TXT/RTF to PDF using iText7
                 iText.Kernel.Pdf.PdfWriter writer = new iText.Kernel.Pdf.PdfWriter(pdfFilePath);
                 iText.Layout.Document document = new iText.Layout.Document(new iText.Kernel.Pdf.PdfDocument(writer));
                 document.Add(new iText.Layout.Element.Paragraph(File.ReadAllText(fullFilePath)));
                 document.Close();
             }

             string relativePdfPath = pdfFilePath.Replace(webRootPath, "").Replace("\\", "/");
             return relativePdfPath;
         }*/


        public async Task<MaterialListViewModel> GetMaterialDetailsByMaterialId(string materialId)
        {
            Material material = await _materialRepository.GetMaterialByMaterialId(Guid.Parse(materialId));
            string filePath = Path.Combine("CourseMaterial", material.FilePath);

            string fileExtension = Path.GetExtension(material.FilePath).ToLower();

            // Check if the file is a DOC/DOCX, PPT/PPTX, TXT or RTF and convert if necessary
            if (fileExtension == ".doc" || fileExtension == ".docx" || fileExtension == ".ppt" || fileExtension == ".pptx" || fileExtension == ".txt" || fileExtension == ".rtf")
            {
                filePath = await ConvertToPdfAsync(filePath, _environment, _contextAccessor);
            }

            string fileUrl = $"{_contextAccessor.HttpContext.Request.Scheme}://{_contextAccessor.HttpContext.Request.Host}{_contextAccessor.HttpContext.Request.PathBase}/wwwroot/{filePath}";

            MaterialListViewModel materialView = new MaterialListViewModel()
            {
                MaterialId = material.MaterialId,
                TopicName = material.Topic.Name,
                MaterialType = material.MaterialType.Type,
                Name = material.Name,
                FilePath = fileUrl,
                Duration = material.Duration,
                IsActive = material.IsActive,
                IsAvailable = material.IsAvailable,
                CreatedAt = material.CreatedAt,
                ModifiedAt = material.ModifiedAt,
                ModifiedBy = material.ModifiedBy,
                CreatedBy = material.CreatedBy
            };
            return materialView;
        }

        // Method to convert DOC/DOCX, PPT/PPTX, TXT, RTF to PDF
        public static async Task<string> ConvertToPdfAsync(string filePath, IWebHostEnvironment environment, IHttpContextAccessor contextAccessor)
        {
            string webRootPath = environment.WebRootPath;
            string fullFilePath = Path.Combine(webRootPath, filePath);

            if (!File.Exists(fullFilePath))
            {
                throw new FileNotFoundException($"The file was not found: {fullFilePath}");
            }

            string fileExtension = Path.GetExtension(filePath).ToLower();
            string pdfFilePath = Path.ChangeExtension(fullFilePath, ".pdf");

            if (fileExtension == ".doc" || fileExtension == ".docx")
            {
                Spire.Doc.Document document = new Spire.Doc.Document();
                document.LoadFromFile(fullFilePath);
                document.SaveToFile(pdfFilePath, Spire.Doc.FileFormat.PDF);
            }
            else if (fileExtension == ".ppt" || fileExtension == ".pptx")
            {
                // Convert PPT/PPTX to PDF using FreeSpire.Presentation
                Spire.Presentation.Presentation presentation = new Spire.Presentation.Presentation();
                presentation.LoadFromFile(fullFilePath);
                presentation.SaveToFile(pdfFilePath, Spire.Presentation.FileFormat.PDF);
            }
            else if (fileExtension == ".txt" || fileExtension == ".rtf")
            {
                // Convert TXT/RTF to PDF using iText7
                iText.Kernel.Pdf.PdfWriter writer = new iText.Kernel.Pdf.PdfWriter(pdfFilePath);
                iText.Layout.Document document = new iText.Layout.Document(new iText.Kernel.Pdf.PdfDocument(writer));
                document.Add(new iText.Layout.Element.Paragraph(File.ReadAllText(fullFilePath)));
                document.Close();
            }

            string relativePdfPath = pdfFilePath.Replace(webRootPath, "").Replace("\\", "/");
            return relativePdfPath;
        }

    }
}
