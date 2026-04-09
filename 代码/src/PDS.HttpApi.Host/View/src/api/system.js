import axios from '@/lib/axios-extension';
export function GetConfiguration() {
    return axios.get('/api/abp/application-configuration');
}