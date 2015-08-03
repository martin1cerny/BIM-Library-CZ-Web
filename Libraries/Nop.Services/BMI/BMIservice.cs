using System.Linq;
using System;
using System.IO;
using System.IO.Compression;
using BLData;
using BLData.Classification;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Diagnostics;
using BLData.PropertySets;
using System.Text.RegularExpressions;

namespace Nop.Services.BMI
{

    [XmlRoot("Model")]
    public class BMIservice
    {

        private int id = 0;

        private static BMIservice instance;

        private BMIservice() { }

        public static BMIservice Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new BMIservice();
                }
                return instance;
            }
        }





        public string getModels()
        {
            BLModel model = null;
            using (var stream = System.IO.File.Open("D:\\_CMS\\ParametryIFC4.blsx", System.IO.FileMode.Open))
            {
                Debug.WriteLine("My debug string here");
                if (System.IO.Path.GetExtension("D:\\_CMS\\ParametryIFC4.blsx").ToLower() == ".blsx")
                {
                    //save as compressed zip file
                    using (var archive = new ZipArchive(stream,
                    ZipArchiveMode.Read))
                    {
                        var entry = archive.Entries.FirstOrDefault(e =>
                        e.Name == "specification.xml");
                        using (var entryStream = entry.Open())
                        {
                            model = BLModel.Open(entryStream);
                            entryStream.Close();
                        }
                    }
                    stream.Close();
                }
                else
                {
                    model = BLModel.Open(stream);
                    stream.Close();
                }
            }
            string sql = "";
            string result = "";
            /*
            List<BLEntityList> BLEntityList = aa.EntityDictionary;
            foreach (BLEntityList BLEntity in BLEntityList)
            {
                Debug.WriteLine("BLEntity" + BLEntity);

                BList<BLModelEntity> BLModelEntityList = BLEntity.Items;
                foreach (BLModelEntity BLModelEntity in BLModelEntityList)
                {
                    Debug.WriteLine("BLModelEntity" + BLModelEntity);
                    BLModel model = BLModelEntity.Model;
                    BLModelInformation BLModelInformation = model.Information;
                    if (BLModelInformation != null && BLModelInformation.Name != null)
                    {
                        Debug.WriteLine("BLModelInformation" + BLModelInformation.Description);
                        result = result + BLModelInformation.Name;
                    }
                    

                }
            }
            */
            
            var classification = model.Get<BLClassification>().FirstOrDefault();
            id = 1;
            int parentId = 0;
            result = result + classification.Name + "<BR>";
            IEnumerable<BLClassificationItem> roots = classification.RootItems;
            foreach (BLClassificationItem item in roots)
            {
                sql = sql + printChildren(item, parentId);
            }
            System.IO.File.WriteAllText(@"C:\VUT\BMI\firstInport.xml", sql);
            return sql;
        }

        string printChildren(BLClassificationItem item, int parentId)
        {
            string result = "";
            string sql = "";
            result = result + "<UL>";
            if (item.Code != null && item.Code.Length > 0)
            {
                result = result + "<LI>Item Code: " + item.Code + "</LI>";
            }

            string name = item.Name;
            string specialName = name;

            //INSERT INTO CATEGORY
            sql = sql + "INSERT INTO CATEGORY (Id, Name, Description,CategoryTemplateId,MetaKeywords,MetaDescription,MetaTitle,ParentCategoryId,PictureId,PageSize,AllowCustomersToSelectPageSize,PageSizeOptions,PriceRanges,ShowOnHomePage,IncludeInTopMenu,HasDiscountsApplied,SubjectToAcl,LimitedToStores,Published,Deleted,DisplayOrder,CreatedOnUtc,UpdatedOnUtc) VALUES ('";
            //id
            sql = sql + id + "','";

            name = Regex.Replace(name, @"Ifc", "");
            name = Regex.Replace(name, @"([a-z])([A-Z])", "$1 $2").Trim();
            //NAME -- todo ! pozriet, ked je Ifc ... az vtedy, inac to neni kategoria !!!
            sql = sql + name + "',";
            //Description 
            sql = sql + "null,";
            //CategoryTemplateId
            sql = sql + "'1',";
            //MetaKeywords
            sql = sql + "null,";
            //MetaDescription
            sql = sql + "null,";
            //MetaTitle
            sql = sql + "null,";
            //parentId
            sql = sql + "'"+parentId+"',";
            //PictureId
            sql = sql + "'" + 0 + "',";
            //PageSize
            sql = sql + "'" + 4 + "',";
            //AllowCustomersToSelectPageSize
            sql = sql + "'" + 1 + "',";
            //PageSizeOptions
            sql = sql + "'8, 4, 12',";
            //PriceRanges
            sql = sql + "null,";
            //ShowOnHomePage
            sql = sql + "'" + 0 + "',";
            //IncludeInTopMenu
            sql = sql + "'" + 1 + "',";
            //[HasDiscountsApplied]
            sql = sql + "'" + 0 + "',";
            //[SubjectToAcl]
            sql = sql + "'" + 0 + "',";
            //[LimitedToStores]
            sql = sql + "'" + 0 + "',";
            //Published
            sql = sql + "'" + 1 + "',";
            //Deleted
            sql = sql + "'" + 0 + "',";
            //DisplayOrder
            sql = sql + "'" + 0 + "',";
            //CreatedOnUtc
            sql = sql + "'<CreatedOnUtc>3/29/2015 6:56:38 PM</CreatedOnUtc>',";
            //UpdatedOnUtc
            sql = sql + "'<CreatedOnUtc>3/29/2015 6:56:38 PM</CreatedOnUtc>'";

            sql = sql + "); <BR>" + Environment.NewLine;

            //UrlRecord
            sql = "";
            sql = sql + "INSERT INTO UrlRecord (Id, EntityId, EntityName, Slug, IsActive, LanguageId) VALUES ('";
            sql = sql + id + "','" + id + "','Category','" + specialName + "','1','0'); <BR>" + Environment.NewLine;

            //Update description
            sql = "";
            sql = sql + "UPDATE CATEGORY SET CategoryId = '" + item.Id + "'WHERE ID = '" + id+"'; <BR>";




            parentId = id;
            id++;

            
            result = result + "<LI><h1>" + name + "</h1></LI>";
           
            //result = result + "Item Description: " + item.Description + "<BR>";

            Debug.WriteLine("Item Code: " + item.Code);
            
            Debug.WriteLine("Item Name: " + item.Name);
            Debug.WriteLine("Item Description: " + item.Description);

            IEnumerable<QuantityPropertySetDef> definitionSets = item.DefinitionSets;

            if (definitionSets.Count() > 0)
            {
                result = result + "<UL>";    
            }

            foreach (QuantityPropertySetDef definition in definitionSets)
            {
                result = result + "Definition Name: " + definition.Name + "<BR>";
                result = result + "Cesky: <BR>";
                BList<NameAlias> definitionAliases = definition.DefinitionAliases;
                foreach (NameAlias nameAlias in definitionAliases)
                {
                    if (nameAlias.Lang == "cs-CZ")
                    {
                        result = result + "<LI>" + nameAlias.Value + "</LI>";
                        break;
                    }
                }

               // definition.na

                if (definition.GetType() == typeof(PropertySetDef)){
                    PropertySetDef newDef = (PropertySetDef)definition;
                    BList<PropertyDef> propertyDefinitions = newDef.PropertyDefinitions;
                    if (propertyDefinitions.Count > 0 )
                    {
                        result = result + "<UL>";     
                    }
                    foreach (PropertyDef propertyDef in propertyDefinitions)
                    {
                        BList<NameAlias> nameAliases = propertyDef.NameAliases;
                        foreach (NameAlias nameAlias in nameAliases)
                        {
                            if (nameAlias.Lang == "cs-CZ")
                            {
                                //result = result + nameAlias.Lang;
                                result = result + "<LI>" + nameAlias.Value + "</LI>";
                            }
                        }
                        BList<NameAlias> definitpropertyDefdefinitionAliasesionAliases = propertyDef.DefinitionAliases;
                        if (definitpropertyDefdefinitionAliasesionAliases.Count > 0)
                        {
                            result = result + "<UL>";
                        }
                        foreach (NameAlias definitpropertyDefdefinitionAliasesionAlias in definitpropertyDefdefinitionAliasesionAliases)
                        {
                            if (definitpropertyDefdefinitionAliasesionAlias.Lang == "cs-CZ")
                            {
                                //result = result + nameAlias.Lang;
                                result = result + "<LI>" + definitpropertyDefdefinitionAliasesionAlias.Value + "</LI>";
                            }
                        }
                        if (definitpropertyDefdefinitionAliasesionAliases.Count > 0)
                        {
                            result = result + "</UL>";
                        }

                        
                    }
                    if (propertyDefinitions.Count > 0)
                    {
                        result = result + "</UL>";  
                    }

                }

                if (definition.GetType() == typeof(QtoSetDef))
                {
                    QtoSetDef newDef = (QtoSetDef)definition;
                    
                    IEnumerable<QuantityPropertyDef> quantityPropertyDefList = newDef.Definitions;
                    result = result + "<h2>quantityPropertyDefList:</h2>";
                    foreach (PropertyDef quantityPropertyDef in quantityPropertyDefList)
                    {
                        result = result + quantityPropertyDef.Definition + "<BR>";
                    }
                }
            }
            if (definitionSets.Count() > 0)
            {
                result = result + "</UL>";    
            }

            IEnumerable<BLClassificationItem> children = item.Children;
           
            
            foreach (BLClassificationItem childrenitem in children)
            {
                sql = sql + printChildren(childrenitem, parentId);
            }
           
            //result = result + "</UL>";
            return sql;
        }



    }



}
