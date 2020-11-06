using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace ConsoleTest
{
    public class AttributeVerificationTest
    {
        public void Handle()
        {
            var dto = new DtoTest();
            dto.Age = -1;
            dto.Name = null;
            var valid = dto.IsValid();
            if (valid.IsVaild)
            {
                Console.WriteLine("pass");
            }
            else
            {
                valid.ErrorMembers.ForEach(e =>
                {
                    Console.WriteLine(e.ErrorMessage);
                });
            }
        }
    }

    public class DtoTest {
        [Required(ErrorMessage ="这个字段必须输入")]
        [StringLength(5, ErrorMessage = "太大")]
        public string Name { get; set; }
        [Range(0,200)]
        public int Age { get; set; }
    }

    public static class ValidatetionHelper {
        public static ValidResult IsValid<T>(this T value)where T:class
        {
            ValidResult result = new ValidResult();
            try
            {
                var validationContext = new ValidationContext(value);
                var results = new List<ValidationResult>();
                var isValid = Validator.TryValidateObject(value, validationContext, results, true);

                if (!isValid)
                {
                    result.IsVaild = false;
                    result.ErrorMembers = new List<ErrorMember>();
                    foreach (var item in results)
                    {
                        result.ErrorMembers.Add(new ErrorMember()
                        {
                            ErrorMessage = item.ErrorMessage,
                            ErrorMemberName = item.MemberNames.FirstOrDefault()
                        });
                    }
                }
                else
                {
                    result.IsVaild = true;
                }
            }
            catch (Exception ex)
            {
                result.IsVaild = false;
                result.ErrorMembers = new List<ErrorMember>();
                result.ErrorMembers.Add(new ErrorMember()
                {
                    ErrorMessage = ex.Message,
                    ErrorMemberName = "Internal error"
                });
            }

            return result;
        }
    }

    public struct ValidResult
    {
        public List<ErrorMember> ErrorMembers { get; set; }
        public bool IsVaild { get; set; }
    }

    public class ErrorMember
    {
        public string ErrorMessage { get; set; }
        public string ErrorMemberName { get; set; }
    }
}
