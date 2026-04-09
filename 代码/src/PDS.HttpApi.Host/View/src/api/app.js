import axios from '@/lib/axios-extension';
export function GetAll() {
    return axios.get("/api/app/user/get");
}
export function GetConfiguration() {

    return axios.get('/api/abp/application-configuration');
};

export function SetCulture(culture) {
    return axios.get(`/api/language/switch?culture=${culture}&uiCulture=${culture}`);
}
export function GetVersion() {
    return axios.get("/api/system/version");
}