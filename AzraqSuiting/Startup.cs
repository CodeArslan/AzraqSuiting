﻿using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AzraqSuiting.Startup))]
namespace AzraqSuiting
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
