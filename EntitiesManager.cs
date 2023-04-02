using EntitiesManager.DM;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntitiesManager
{
    public class EntitiesManager
    {
        private IDictionary<string, EntityTemplate> templates;

        public EntitiesManager(IEnumerable<EntityTemplate> entitiesList)
        {
            templates = entitiesList.ToDictionary((entity) => entity.Type, (entity) => entity);
        }

        public JObject MergeResults(string entityType, IDictionary<string, JObject> resultByInterfaces)
        {
            if (!templates.ContainsKey(entityType))
                throw new InvalidOperationException($"Entity type {entityType} dose not exsist");

            return CreateMergeObject(resultByInterfaces, templates[entityType]);
        }

        private JObject CreateMergeObject(IDictionary<string, JObject> resultByInterfaces, EntityTemplate template)
        {
            JObject result = new JObject();
            AddValuesFiledToResult(result, template, resultByInterfaces);

            foreach (var filed in template.ObjectsFileds)
            {
                IDictionary<string, JObject> objectByInterfaces = SelectObjectFromResult(resultByInterfaces, filed.Type);
                if (objectByInterfaces.Count > 0)
                    result[filed.Type] = CreateMergeObject(objectByInterfaces, filed);
            }

            return result;
        }

        private void AddValuesFiledToResult(JObject result, EntityTemplate template, IDictionary<string, JObject> resultByInterfaces)
        {
            foreach (var filed in template.ValuesFileds)
            {
                foreach (var interfaceName in filed.Priorities)
                {
                    if (resultByInterfaces.ContainsKey(interfaceName) && resultByInterfaces[interfaceName].ContainsKey(filed.Key))
                    {
                        var selected = resultByInterfaces[interfaceName][filed.Key];
                            result[filed.Key] = selected;
                            break;
                    }
                }
            }
        }

        private IDictionary<string, JObject> SelectObjectFromResult(IDictionary<string, JObject> resultByInterfaces, string objectFiledKey)
        {
            IDictionary<string, JObject> result = new Dictionary<string, JObject>();

            foreach (var interfacesResult in resultByInterfaces)
            {
                if (interfacesResult.Value.ContainsKey(objectFiledKey))
                {
                    var selected = interfacesResult.Value[objectFiledKey];
                    if (selected.Type == JTokenType.Object)
                        result[interfacesResult.Key] = (JObject)selected;
                    else
                        throw new InvalidDataException($"{selected.Path} shuold be object not {selected.Type}");
                }
            }

            return result;
        }
    }
}
