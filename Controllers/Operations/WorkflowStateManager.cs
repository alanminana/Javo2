// Services/Operations/WorkflowStateManager.cs
using Javo2.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Javo2.Services.Operations
{
    public interface IWorkflowStateManager<TState> where TState : Enum
    {
        bool CanTransitionTo(TState fromState, TState toState);
        IEnumerable<TState> GetValidTransitions(TState fromState);
        bool IsEditableState(TState state);
        bool IsDeletableState(TState state);
        string GetStateDescription(TState state);
    }

    public class WorkflowStateManager<TState> : IWorkflowStateManager<TState> where TState : Enum
    {
        private readonly Dictionary<TState, StateConfiguration<TState>> _stateConfigurations;
        private readonly ILogger<WorkflowStateManager<TState>> _logger;

        public WorkflowStateManager(ILogger<WorkflowStateManager<TState>> logger)
        {
            _stateConfigurations = new Dictionary<TState, StateConfiguration<TState>>();
            _logger = logger;
        }

        public void ConfigureState(TState state, Action<StateConfiguration<TState>> configure)
        {
            var config = new StateConfiguration<TState>(state);
            configure(config);
            _stateConfigurations[state] = config;
        }

        public bool CanTransitionTo(TState fromState, TState toState)
        {
            try
            {
                if (_stateConfigurations.TryGetValue(fromState, out var config))
                {
                    return config.ValidTransitions.Contains(toState);
                }

                _logger.LogWarning("No hay configuración definida para el estado: {State}", fromState);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar transición de {FromState} a {ToState}", fromState, toState);
                return false;
            }
        }

        public IEnumerable<TState> GetValidTransitions(TState fromState)
        {
            try
            {
                if (_stateConfigurations.TryGetValue(fromState, out var config))
                {
                    return config.ValidTransitions;
                }

                _logger.LogWarning("No hay configuración definida para el estado: {State}", fromState);
                return Enumerable.Empty<TState>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener transiciones válidas para {State}", fromState);
                return Enumerable.Empty<TState>();
            }
        }

        public bool IsEditableState(TState state)
        {
            try
            {
                if (_stateConfigurations.TryGetValue(state, out var config))
                {
                    return config.IsEditable;
                }

                _logger.LogWarning("No hay configuración definida para el estado: {State}", state);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar si el estado {State} es editable", state);
                return false;
            }
        }

        public bool IsDeletableState(TState state)
        {
            try
            {
                if (_stateConfigurations.TryGetValue(state, out var config))
                {
                    return config.IsDeletable;
                }

                _logger.LogWarning("No hay configuración definida para el estado: {State}", state);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar si el estado {State} es eliminable", state);
                return false;
            }
        }

        public string GetStateDescription(TState state)
        {
            try
            {
                if (_stateConfigurations.TryGetValue(state, out var config))
                {
                    return config.Description ?? state.ToString();
                }

                return state.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener descripción del estado {State}", state);
                return state.ToString();
            }
        }
    }

    public class StateConfiguration<TState> where TState : Enum
    {
        public TState State { get; }
        public List<TState> ValidTransitions { get; }
        public bool IsEditable { get; set; }
        public bool IsDeletable { get; set; }
        public string Description { get; set; }

        public StateConfiguration(TState state)
        {
            State = state;
            ValidTransitions = new List<TState>();
            IsEditable = false;
            IsDeletable = false;
        }

        public StateConfiguration<TState> AllowTransitionTo(params TState[] states)
        {
            ValidTransitions.AddRange(states);
            return this;
        }

        public StateConfiguration<TState> AllowEdit(bool allow = true)
        {
            IsEditable = allow;
            return this;
        }

        public StateConfiguration<TState> AllowDelete(bool allow = true)
        {
            IsDeletable = allow;
            return this;
        }

        public StateConfiguration<TState> WithDescription(string description)
        {
            Description = description;
            return this;
        }
    }

    // Implementaciones específicas para diferentes tipos de operaciones

    public class VentaWorkflowStateManager : WorkflowStateManager<EstadoVenta>
    {
        public VentaWorkflowStateManager(ILogger<WorkflowStateManager<EstadoVenta>> logger) : base(logger)
        {
            ConfigureVentaStates();
        }

        private void ConfigureVentaStates()
        {
            ConfigureState(EstadoVenta.Borrador, config => config
                .AllowTransitionTo(EstadoVenta.PendienteDeAutorizacion, EstadoVenta.Cancelada)
                .AllowEdit()
                .AllowDelete()
                .WithDescription("Borrador - En proceso de creación"));

            ConfigureState(EstadoVenta.PendienteDeAutorizacion, config => config
                .AllowTransitionTo(EstadoVenta.Autorizada, EstadoVenta.Rechazada)
                .WithDescription("Pendiente de autorización"));

            ConfigureState(EstadoVenta.Autorizada, config => config
                .AllowTransitionTo(EstadoVenta.Entregada, EstadoVenta.Cancelada)
                .WithDescription("Autorizada - Lista para entrega"));

            ConfigureState(EstadoVenta.Entregada, config => config
                .WithDescription("Entregada - Venta completada"));

            ConfigureState(EstadoVenta.Rechazada, config => config
                .AllowTransitionTo(EstadoVenta.Borrador)
                .AllowDelete()
                .WithDescription("Rechazada"));

            ConfigureState(EstadoVenta.Cancelada, config => config
                .WithDescription("Cancelada"));
        }
    }

    public class CompraWorkflowStateManager : WorkflowStateManager<EstadoCompra>
    {
        public CompraWorkflowStateManager(ILogger<WorkflowStateManager<EstadoCompra>> logger) : base(logger)
        {
            ConfigureCompraStates();
        }

        private void ConfigureCompraStates()
        {
            ConfigureState(EstadoCompra.Borrador, config => config
                .AllowTransitionTo(EstadoCompra.Confirmada, EstadoCompra.Cancelada)
                .AllowEdit()
                .AllowDelete()
                .WithDescription("Borrador"));

            ConfigureState(EstadoCompra.Confirmada, config => config
                .AllowTransitionTo(EstadoCompra.EnProceso, EstadoCompra.Cancelada)
                .WithDescription("Confirmada"));

            ConfigureState(EstadoCompra.EnProceso, config => config
                .AllowTransitionTo(EstadoCompra.Completada, EstadoCompra.Cancelada)
                .WithDescription("En proceso"));

            ConfigureState(EstadoCompra.Completada, config => config
                .WithDescription("Completada"));

            ConfigureState(EstadoCompra.Cancelada, config => config
                .WithDescription("Cancelada"));
        }
    }
}