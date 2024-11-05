﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Shuttle.Esb {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Shuttle.Esb.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot add subscription as a valid instance of the `MessageTypes` list cannot be reached..
        /// </summary>
        public static string AddSubscriptionException {
            get {
                return ResourceManager.GetString("AddSubscriptionException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not find assembly &apos;{0}&apos; during operation &apos;{1}&apos;..
        /// </summary>
        public static string AssemblyNotFound {
            get {
                return ResourceManager.GetString("AssemblyNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not resolve source uri &apos;{0}&apos;..
        /// </summary>
        public static string CouldNotResolveSourceUriException {
            get {
                return ResourceManager.GetString("CouldNotResolveSourceUriException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Only one instance of the DeferredMessageProcessor should be created.  Check that the ProcessorThreadPool for the DeferredMessageProcessorFactory is not using more than 1 thread..
        /// </summary>
        public static string DeferredMessageProcessorInstanceException {
            get {
                return ResourceManager.GetString("DeferredMessageProcessorInstanceException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot defer messages when the inbox is a stream..
        /// </summary>
        public static string DeferStreamException {
            get {
                return ResourceManager.GetString("DeferStreamException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There is already a delegate mapped to message type &apos;{0}&apos;..
        /// </summary>
        public static string DelegateAlreadyMappedException {
            get {
                return ResourceManager.GetString("DelegateAlreadyMappedException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This is already a configuration registered for endpoint with name &apos;{0}&apos;..
        /// </summary>
        public static string DuplicateQueueConfigurationNameException {
            get {
                return ResourceManager.GetString("DuplicateQueueConfigurationNameException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not create a HandlerContext for type &apos;{0}&apos;..
        /// </summary>
        public static string HandlerContextConstructorMissingException {
            get {
                return ResourceManager.GetString("HandlerContextConstructorMissingException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Handler type &apos;{0}&apos; does not have the required `ProcessMessage` method that handles message type &apos;{1}&apos;..
        /// </summary>
        public static string HandlerMessageMethodMissingException {
            get {
                return ResourceManager.GetString("HandlerMessageMethodMissingException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The service bus is running in &apos;{0}&apos; mode.  To stop please call the &apos;{1}&apos; method..
        /// </summary>
        public static string IncorrectStopCalledException {
            get {
                return ResourceManager.GetString("IncorrectStopCalledException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Only scheme &apos;{0}&apos; is supported.  The given uri &apos;{1}&apos; is not supported..
        /// </summary>
        public static string InvalidSchemeException {
            get {
                return ResourceManager.GetString("InvalidSchemeException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The value &apos;{0}&apos; is not a valid URI for &apos;{1}&apos;..
        /// </summary>
        public static string InvalidUriException {
            get {
                return ResourceManager.GetString("InvalidUriException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The delegate must contain at least a parameter of type `IHandlerContext&lt;TMessage&gt;`..
        /// </summary>
        public static string MessageHandlerTypeException {
            get {
                return ResourceManager.GetString("MessageHandlerTypeException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No handler has been registered for message type &apos;{0}&apos;.  The message (id &apos;{1}&apos;) has been moved to error queue &apos;{2}&apos;..
        /// </summary>
        public static string MessageNotHandledFailure {
            get {
                return ResourceManager.GetString("MessageNotHandledFailure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No handler has been registered for message type &apos;{0}&apos;.  The message (id &apos;{1}&apos;) has been ignored..
        /// </summary>
        public static string MessageNotHandledIgnored {
            get {
                return ResourceManager.GetString("MessageNotHandledIgnored", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No handler has been registered for message type &apos;{0}&apos;.  The message (id &apos;{1}&apos;) could not be moved to the error queue as no error queue has been set in the pipeline state..
        /// </summary>
        public static string MessageNotHandledMissingErrorQueueFailure {
            get {
                return ResourceManager.GetString("MessageNotHandledMissingErrorQueueFailure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Message of type &apos;{0}&apos; has been routed to more than one endpoint: {1}.
        /// </summary>
        public static string MessageRoutedToMoreThanOneEndpoint {
            get {
                return ResourceManager.GetString("MessageRoutedToMoreThanOneEndpoint", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No route could be found for message of type &apos;{0}&apos;..
        /// </summary>
        public static string MessageRouteNotFound {
            get {
                return ResourceManager.GetString("MessageRouteNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to If a message route is defined it requires at least one specification..
        /// </summary>
        public static string MessageRoutesRequireSpecificationException {
            get {
                return ResourceManager.GetString("MessageRoutesRequireSpecificationException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to (no error queue).
        /// </summary>
        public static string NoErrorQueue {
            get {
                return ResourceManager.GetString("NoErrorQueue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Queue &apos;{0}&apos; does not implement interface &apos;{1}&apos;..
        /// </summary>
        public static string NotImplementedOnQueue {
            get {
                return ResourceManager.GetString("NotImplementedOnQueue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Queue configuration with name &apos;{0}&apos; requires a value for &apos;{1}&apos;..
        /// </summary>
        public static string QueueConfigurationItemException {
            get {
                return ResourceManager.GetString("QueueConfigurationItemException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The configuration name is empty..
        /// </summary>
        public static string QueueConfigurationNameException {
            get {
                return ResourceManager.GetString("QueueConfigurationNameException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Queue factory with type &apos;{0}&apos; create returned (null) for uri &apos;{1}&apos;..
        /// </summary>
        public static string QueueFactoryCreatedNullQueue {
            get {
                return ResourceManager.GetString("QueueFactoryCreatedNullQueue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No queue factory has been registered for scheme &apos;{0}&apos;..
        /// </summary>
        public static string QueueFactoryNotFoundException {
            get {
                return ResourceManager.GetString("QueueFactoryNotFoundException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Required options &apos;{0}&apos; have not been provided..
        /// </summary>
        public static string RequiredOptionsMissingException {
            get {
                return ResourceManager.GetString("RequiredOptionsMissingException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Required queue uri &apos;{0}&apos; has not been configured.  Please check your application settings or your code if you implemented it explicitly..
        /// </summary>
        public static string RequiredQueueUriMissingException {
            get {
                return ResourceManager.GetString("RequiredQueueUriMissingException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot send reply as the provided transport message received has no inbox work queue uri..
        /// </summary>
        public static string SendReplyException {
            get {
                return ResourceManager.GetString("SendReplyException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You cannot send a message to this endpoint (Local) since this endpoint has no inbox..
        /// </summary>
        public static string SendToSelfException {
            get {
                return ResourceManager.GetString("SendToSelfException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The service bus instance has already been started..
        /// </summary>
        public static string ServiceBusInstanceAlreadyStarted {
            get {
                return ResourceManager.GetString("ServiceBusInstanceAlreadyStarted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The service bus instance has not yet been started..
        /// </summary>
        public static string ServiceBusInstanceNotStarted {
            get {
                return ResourceManager.GetString("ServiceBusInstanceNotStarted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The service bus options asynchronous value is &apos;true&apos;.  Cannot start the service bus synchronously.  Please call the `StartAsync()` method..
        /// </summary>
        public static string ServiceBusStartAsynchronousException {
            get {
                return ResourceManager.GetString("ServiceBusStartAsynchronousException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The service bus options asynchronous value is &apos;false&apos;.  Cannot start the service bus asynchronously.  Please call the `Start()` method..
        /// </summary>
        public static string ServiceBusStartSynchronousException {
            get {
                return ResourceManager.GetString("ServiceBusStartSynchronousException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Attempted to subscribe to messages but there is no inbox configured..
        /// </summary>
        public static string SubscribeWithNoInboxException {
            get {
                return ResourceManager.GetString("SubscribeWithNoInboxException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Transport message with id &apos;{0}&apos; will be deferred until &apos;{1}&apos;..
        /// </summary>
        public static string TraceTransportMessageDeferred {
            get {
                return ResourceManager.GetString("TraceTransportMessageDeferred", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The recipient has already been set for the transport message..
        /// </summary>
        public static string TransportMessageRecipientException {
            get {
                return ResourceManager.GetString("TransportMessageRecipientException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot get type &apos;{0}&apos; for TypeListMessageRouteSpecification..
        /// </summary>
        public static string TypeListMessageRouteSpecificationUnknownType {
            get {
                return ResourceManager.GetString("TypeListMessageRouteSpecificationUnknownType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unknown message route specification &apos;{0}&apos;.  Cannot create the specification..
        /// </summary>
        public static string UnknownMessageRouteSpecification {
            get {
                return ResourceManager.GetString("UnknownMessageRouteSpecification", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Expected uri format &apos;{0}&apos; but received &apos;{1}&apos;..
        /// </summary>
        public static string UriFormatException {
            get {
                return ResourceManager.GetString("UriFormatException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The IUriResolver type &apos;{0}&apos; could not resolve name &apos;{1}&apos;..
        /// </summary>
        public static string UriNameNotFoundException {
            get {
                return ResourceManager.GetString("UriNameNotFoundException", resourceCulture);
            }
        }
    }
}
