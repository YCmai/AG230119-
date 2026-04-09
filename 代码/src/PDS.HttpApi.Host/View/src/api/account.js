import axios from '@/lib/axios-extension';
export function Login(account) {
    return axios.post('/api/account/login', account);
}
export function Reset(input) {
    return axios.post('/api/wms/user/reset-password', input);
}
export function Logout() {
    return axios.get('/api/account/logout');
}