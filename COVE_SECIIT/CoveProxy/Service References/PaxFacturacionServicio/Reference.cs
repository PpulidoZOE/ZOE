﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CoveProxy.PaxFacturacionServicio {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="https://www.paxfacturacion.com.mx:453", ConfigurationName="PaxFacturacionServicio.wcfRecepcionASMXSoap")]
    public interface wcfRecepcionASMXSoap {
        
        // CODEGEN: Generating message contract since element name psComprobante from namespace https://www.paxfacturacion.com.mx:453 is not marked nillable
        [System.ServiceModel.OperationContractAttribute(Action="https://www.paxfacturacion.com.mx:453/fnEnviarXML", ReplyAction="*")]
        CoveProxy.PaxFacturacionServicio.fnEnviarXMLResponse fnEnviarXML(CoveProxy.PaxFacturacionServicio.fnEnviarXMLRequest request);
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class fnEnviarXMLRequest {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="fnEnviarXML", Namespace="https://www.paxfacturacion.com.mx:453", Order=0)]
        public CoveProxy.PaxFacturacionServicio.fnEnviarXMLRequestBody Body;
        
        public fnEnviarXMLRequest() {
        }
        
        public fnEnviarXMLRequest(CoveProxy.PaxFacturacionServicio.fnEnviarXMLRequestBody Body) {
            this.Body = Body;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.Runtime.Serialization.DataContractAttribute(Namespace="https://www.paxfacturacion.com.mx:453")]
    public partial class fnEnviarXMLRequestBody {
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=0)]
        public string psComprobante;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=1)]
        public string psTipoDocumento;
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=2)]
        public int pnId_Estructura;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=3)]
        public string sNombre;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=4)]
        public string sContraseña;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=5)]
        public string sVersion;
        
        public fnEnviarXMLRequestBody() {
        }
        
        public fnEnviarXMLRequestBody(string psComprobante, string psTipoDocumento, int pnId_Estructura, string sNombre, string sContraseña, string sVersion) {
            this.psComprobante = psComprobante;
            this.psTipoDocumento = psTipoDocumento;
            this.pnId_Estructura = pnId_Estructura;
            this.sNombre = sNombre;
            this.sContraseña = sContraseña;
            this.sVersion = sVersion;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class fnEnviarXMLResponse {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="fnEnviarXMLResponse", Namespace="https://www.paxfacturacion.com.mx:453", Order=0)]
        public CoveProxy.PaxFacturacionServicio.fnEnviarXMLResponseBody Body;
        
        public fnEnviarXMLResponse() {
        }
        
        public fnEnviarXMLResponse(CoveProxy.PaxFacturacionServicio.fnEnviarXMLResponseBody Body) {
            this.Body = Body;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.Runtime.Serialization.DataContractAttribute(Namespace="https://www.paxfacturacion.com.mx:453")]
    public partial class fnEnviarXMLResponseBody {
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=0)]
        public string fnEnviarXMLResult;
        
        public fnEnviarXMLResponseBody() {
        }
        
        public fnEnviarXMLResponseBody(string fnEnviarXMLResult) {
            this.fnEnviarXMLResult = fnEnviarXMLResult;
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface wcfRecepcionASMXSoapChannel : CoveProxy.PaxFacturacionServicio.wcfRecepcionASMXSoap, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class wcfRecepcionASMXSoapClient : System.ServiceModel.ClientBase<CoveProxy.PaxFacturacionServicio.wcfRecepcionASMXSoap>, CoveProxy.PaxFacturacionServicio.wcfRecepcionASMXSoap {
        
        public wcfRecepcionASMXSoapClient() {
        }
        
        public wcfRecepcionASMXSoapClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public wcfRecepcionASMXSoapClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public wcfRecepcionASMXSoapClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public wcfRecepcionASMXSoapClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        CoveProxy.PaxFacturacionServicio.fnEnviarXMLResponse CoveProxy.PaxFacturacionServicio.wcfRecepcionASMXSoap.fnEnviarXML(CoveProxy.PaxFacturacionServicio.fnEnviarXMLRequest request) {
            return base.Channel.fnEnviarXML(request);
        }
        
        public string fnEnviarXML(string psComprobante, string psTipoDocumento, int pnId_Estructura, string sNombre, string sContraseña, string sVersion) {
            CoveProxy.PaxFacturacionServicio.fnEnviarXMLRequest inValue = new CoveProxy.PaxFacturacionServicio.fnEnviarXMLRequest();
            inValue.Body = new CoveProxy.PaxFacturacionServicio.fnEnviarXMLRequestBody();
            inValue.Body.psComprobante = psComprobante;
            inValue.Body.psTipoDocumento = psTipoDocumento;
            inValue.Body.pnId_Estructura = pnId_Estructura;
            inValue.Body.sNombre = sNombre;
            inValue.Body.sContraseña = sContraseña;
            inValue.Body.sVersion = sVersion;
            CoveProxy.PaxFacturacionServicio.fnEnviarXMLResponse retVal = ((CoveProxy.PaxFacturacionServicio.wcfRecepcionASMXSoap)(this)).fnEnviarXML(inValue);
            return retVal.Body.fnEnviarXMLResult;
        }
    }
}
