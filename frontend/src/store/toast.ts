import { defineStore } from 'pinia';
import { ref } from 'vue';

export const useToastStore = defineStore('toast', () => {
    const toasts = ref<any[]>([]);

    const show = (message: string, type: 'success' | 'error' | 'info' = 'info') => {
        const id = Date.now();
        toasts.value.push({ id, message, type });
        
        // 5 saniye sonra bildirimi kaldır
        setTimeout(() => {
            toasts.value = toasts.value.filter(t => t.id !== id);
        }, 5000);
    };

    const success = (msg: string) => show(msg, 'success');
    const error = (msg: string) => show(msg, 'error');
    const info = (msg: string) => show(msg, 'info');

    return { toasts, success, error, info };
});
