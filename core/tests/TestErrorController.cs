namespace Glow.Core.Tests
{
    [Microsoft.AspNetCore.Mvc.Route("api/test/error")]
    public class Testcontroller : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        [Microsoft.AspNetCore.Mvc.HttpGet]
        public Microsoft.AspNetCore.Mvc.ActionResult Get()
        {
            throw new System.Exception("TEST ERROR");
        }
    }
}
