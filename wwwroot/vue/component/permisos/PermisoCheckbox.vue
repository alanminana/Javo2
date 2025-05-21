<template>
    <div class="form-check">
        <input type="checkbox"
               :id="`permiso_${permiso.id}`"
               :name="`PermisosSeleccionados[${index}]`"
               :value="permiso.id"
               class="form-check-input"
               v-model="isChecked">
        <label class="form-check-label" :for="`permiso_${permiso.id}`">
            {{ permiso.nombre }}
            <small class="text-muted d-block">{{ permiso.codigo }}</small>
        </label>
    </div>
</template>

<script>
import { ref, watch } from 'vue';

export default {
  name: 'PermisoCheckbox',
  props: {
    permiso: {
      type: Object,
      required: true
    },
    modelValue: {
      type: Array,
      default: () => []
    },
    index: {
      type: Number,
      required: true
    }
  },
  emits: ['update:modelValue'],
  setup(props, { emit }) {
    const isChecked = ref(props.modelValue.includes(props.permiso.id));

    // Actualizar el estado al cambiar las props
    watch(() => props.modelValue, (newValue) => {
      isChecked.value = newValue.includes(props.permiso.id);
    });

    // Emitir cambios cuando cambia el checkbox
    watch(isChecked, (newValue) => {
      const updatedValue = [...props.modelValue];

      if (newValue && !updatedValue.includes(props.permiso.id)) {
        updatedValue.push(props.permiso.id);
      } else if (!newValue && updatedValue.includes(props.permiso.id)) {
        const index = updatedValue.indexOf(props.permiso.id);
        updatedValue.splice(index, 1);
      }

      emit('update:modelValue', updatedValue);
    });

    return {
      isChecked
    };
  }
};
</script>