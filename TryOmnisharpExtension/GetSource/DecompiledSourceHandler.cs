﻿using System.Threading.Tasks;

namespace TryOmnisharpExtension.GetSource
{
    public class DecompiledSourceHandler : HandlerBase<DecompiledSourceRequest, DecompiledSourceResponse>
    {
        private readonly IlSpyDecompiledSourceCommandFactory _commandFactory;

        public DecompiledSourceHandler(IlSpyDecompiledSourceCommandFactory commandFactory)
        {
            _commandFactory =  commandFactory;
        }
        
        public override Task<DecompiledSourceResponse> Handle(DecompiledSourceRequest request)
        {
            var response = _commandFactory.Find(request);
            return Task.FromResult(response);
        }
    }
}