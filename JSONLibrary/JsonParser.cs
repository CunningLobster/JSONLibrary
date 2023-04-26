using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSONLibrary
{
    public static class JsonParser
    {
        #region Json
        /// <summary>
        /// Получить свойство из Json строки
        /// </summary>
        /// <param name="source">Json строка</param>
        /// <param name="propertyName">Имя свойства</param>
        /// <param name="breakets">Контрольная сумма скобок(!!!Оставить значение по умолчанию)</param>
        /// <returns>Строка со свойством и значением в формате Json</returns>
        public static string GetJsonProperty(string source, string propertyName, int breakets = -1)
        {
            //Строка в формате JSON
            string formattedPropName = "\"" + propertyName + "\"";

            //Найти первое вхождение строки и посчитать контрольную сумму скобок
            string sub = source.Substring(source.IndexOf(formattedPropName));
            string leftString = source.Substring(0, source.Length - sub.Length);
            foreach (char c in leftString)
            {
                if (c == '{') breakets++;
                if (c == '}') breakets--;
            }

            //Если контрольная сумма скобок равна 0, то нашлось искомое вхождение
            if (breakets == 0)
                return CutPropertyString(sub);
            else
                return GetJsonProperty(sub.Substring(formattedPropName.Length),
                                       propertyName, breakets);
        }

        /// <summary>
        /// Получить свойство из Json строки
        /// </summary>
        /// <param name="source">Json строка</param>
        /// <param name="propertyNames">Имена свойств, упорядоченные в порядке наследования</param>
        /// <returns></returns>
        public static string GetJsonProperty(string source, string[] propertyNames)
        {
            string finalProperty = source;
            foreach (string name in propertyNames)
                finalProperty = GetJsonProperty(finalProperty, name);
            return finalProperty;
        }


        /// <summary>
        /// Обрезать конец строки со свойством
        /// </summary>
        /// <param name="source">Образаемая строка</param>
        /// <returns>Строка со свойством и значением в формате Json</returns>
        private static string CutPropertyString(string source)
        {
            int colonIndex = source.IndexOf(":");
            if (source[colonIndex + 1] == '{')
                return CutPropWithBreakets(source, '{', '}');
            if (source[colonIndex + 1] == '[')
                return CutPropWithBreakets(source, '[', ']');
            else
                return CutSingleProperty(source);
        }

        /// <summary>
        /// Обрезать строку одиночного Json свойства
        /// </summary>
        /// <param name="source">Обрезаемая строка</param>
        /// <returns>Строка со свойством и значением в формате Json</returns>
        private static string CutSingleProperty(string source)
        {
            int colonIndex = source.IndexOf(":");
            string propBody = source.Substring(colonIndex);
            string propHeader = source.Substring(0, source.IndexOf(propBody));

            return (propHeader + GetSinglePropertyValue(propBody));
        }

        /// <summary>
        /// Обрезать строку со сложным Json свойством
        /// </summary>
        /// <param name="source">Обрезаемая строка</param>
        /// <param name="openBreaket">Открывающая скобка</param>
        /// <param name="closeBreaket">Закрывающая скобка</param>
        /// <returns>Строка со свойством и значением в формате Json</returns>
        private static string CutPropWithBreakets(string source,
                                                  char openBreaket, char closeBreaket)
        {
            //Получаем блок
            string blockBody = source.Substring(source.IndexOf(openBreaket));
            string blockHeader = source.Substring(0, source.IndexOf(blockBody));

            int breakets = 0;
            string result = String.Empty;
            foreach (char c in blockBody)
            {
                result += c;
                if (c == openBreaket) breakets++;
                if (c == closeBreaket) breakets--;
                if (breakets == 0) break;
            }

            return blockHeader + result;
        }

        /// <summary>
        /// Получить значение Json свойства
        /// </summary>
        /// <param name="property">Свойство</param>
        /// <returns>Значение</returns>
        public static string GetJsonPropertyValue(string property)
        {
            int colonIndex = property.IndexOf(":");
            string propBody = property.Substring(colonIndex + 1);

            //Убрать внешние ковычки
            propBody = propBody.TrimStart('\"');
            if (propBody[propBody.Length - 1] == '\"')
                propBody = propBody.Substring(0, propBody.Length - 1);

            return propBody;
        }

        /// <summary>
        /// Отобразить значение Json свойства без "экранированных" ковычек (Использовать для отображения значения на контроле)
        /// </summary>
        /// <param name="property">Json свойство</param>
        /// <returns>Значение свойства</returns>
        public static string DisplayJsonPropertyValue(string property)
        {
            string valueToDisplay = GetJsonPropertyValue(property);

            //Заменить \" на "
            if (valueToDisplay.Contains("\\\""))
                valueToDisplay = valueToDisplay.Replace("\\\"", "\"");

            return valueToDisplay;
        }

        /// <summary>
        /// Получить элемент Json свойства-коллекции
        /// </summary>
        /// <param name="property">Свойство в формате Json</param>
        /// <param name="index">Индекс элемента</param>
        /// <returns>Элемент Json свойства-коллекции</returns>
        public static string GetJsonListElement(string property, int index)
        {
            string propBody = string.Empty;

            if (property.Trim()[0] == '[')
                propBody = property.TrimStart('[').TrimEnd(']');
            else if (property[property.IndexOf(":") + 1] == '[')
                propBody = property.Substring(property.IndexOf(":") + 1).TrimStart('[').TrimEnd(']');
            else
                throw new Exception("Не является свойством-коллекцией");

            int startIndex = 0;
            string result = string.Empty;
            for (int i = 0; i <= index; i++)
            {
                propBody = propBody.Substring(startIndex).TrimStart();
                if (string.IsNullOrEmpty(propBody))
                    throw new IndexOutOfRangeException();
                else
                {
                    if (propBody[0] == '{')
                        result = CutPropWithBreakets(propBody, '{', '}');
                    else if (propBody[0] == '[')
                        result = CutPropWithBreakets(propBody, '[', ']');
                    else
                        result = GetSinglePropertyValue(propBody);

                    if (i == index)
                        result = result.Trim(',');
                    else
                    {
                        startIndex = result.Length + 1;
                        result = string.Empty;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Получить длину Json свойства-коллекции
        /// </summary>
        /// <param name="property">Свойство-коллекция</param>
        /// <returns>Длина свойства-коллекции</returns>
        public static int GetJsonListCount(string property)
        {
            int count = 0;
            string element = GetJsonListElement(property, count);

            while (element != String.Empty)
            {
                try
                {
                    count++;
                    element = GetJsonListElement(property, count);
                }
                catch
                {
                    break;
                }
            }
            return count;
        }

        /// <summary>
        /// Получить значение одиночного свойства Json
        /// </summary>
        /// <param name="value">Тело свойства</param>
        /// <returns>Значение</returns>
        public static string GetSinglePropertyValue(string value)
        {
            string result = string.Empty;
            //Если открывается ковычка
            if (value[0] == '\"')
            {
                //Получаем значение с закрытием ковычки
                int quotes = 0;
                foreach (char c in value)
                {
                    result += c;
                    if (c == '\"') quotes++;
                    if (quotes == 2) break;
                }
            }
            else
            {
                //Получаем значение до запятой или до закрытой скобки
                foreach (char c in value)
                {
                    if (c == ',' || c == '}' || c == ']') break;
                    result += c;
                }
            }
            return result;
        }

        /// <summary>
        /// Вернуть Json строку на основе source без удаленного свойства
        /// </summary>
        /// <param name="source"></param>
        /// <param name="propertyName"></param>
        /// <returns>Json строка на основе source с удаленным свойством</returns>
        public static string RemoveJsonProperty(string source, string propertyName)
        {
            string propertyString = GetJsonProperty(source, propertyName);
            int propertyIndex = source.IndexOf(propertyString);
            string left = source.Substring(0, propertyIndex);
            string right = source.Substring(propertyIndex + propertyString.Length);

            string result = left + right;
            //Убрать лишнюю запятую
            if (source[propertyIndex - 1] != ',')
                return result.Remove(propertyIndex, 1);
            else
                return result.Remove(propertyIndex - 1, 1);
        }

        /// <summary>
        /// Установить новое значение Json свойства
        /// </summary>
        /// <param name="source">Строка в формате Json</param>
        /// <param name="propertyName">Имя свойства</param>
        /// <param name="value">Устанавливаемое значение</param>
        /// <param name="quotes">true, если значение должно быть помещено в кавычки</param>
        public static string SetJsonProperty(string source, string propertyName, string value, bool quotes)
        {
            int propertyIndex = source.IndexOf(GetJsonProperty(source, propertyName));
            string cutSource = RemoveJsonProperty(source, propertyName);
            if (quotes) value = "\"" + value + "\"";
            string propertyToInsert = $"\"{propertyName}\":{value}";

            string left = string.Empty;
            string right = string.Empty;
            if (cutSource[propertyIndex - 1] == '{' || cutSource[propertyIndex - 1] == '[')
            {
                left = cutSource.Substring(0, propertyIndex);
                right = cutSource.Substring(propertyIndex);
            }
            else
            {
                left = cutSource.Substring(0, propertyIndex - 1);
                right = cutSource.Substring(propertyIndex - 1);
            }

            if (cutSource[propertyIndex - 1] == '{' || cutSource[propertyIndex - 1] == '[')
                source = left + propertyToInsert + "," + right;
            else
                source = left + "," + propertyToInsert + right;

            return source;
        }

        public static string SetJsonListElement(string source, string value, int index)
        {
            int placeToInsert = source.IndexOf(GetJsonListElement(source, index));
            int gap = GetJsonListElement(source, index).Length;

            string left = source.Substring(0, placeToInsert);
            string right = source.Substring(placeToInsert + gap);

            return left + value + right;
        }

        public static string AddJsonListElement(string jsonList, string elementToAdd)
        {
            int lastBreaketIndex = jsonList.LastIndexOf("]");
            if (lastBreaketIndex == -1) return jsonList;

            int count = GetJsonListCount(jsonList);
            if (count == 0)
                jsonList = jsonList.Insert(lastBreaketIndex, elementToAdd);
            else if (count > 0)
                jsonList = jsonList.Insert(lastBreaketIndex, "," + elementToAdd);
            return jsonList;
        }

        #endregion

    }
}
