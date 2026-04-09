import axios from '@/lib/axios-extension';
export function GetPermissions(id) {
    return axios.get(`/api/permission-management/permissions?providerName=U&providerKey=${id}`);
}
export function Get(providerName, providerKey) {
    return axios.get(`/api/permission-management/permissions?providerName=${providerName}&providerKey=${providerKey}`);
}
export function Grant(providerKey, providetName, grantedPermissions) {
    return axios.put('/api/permission-management/permissions?providerName=' + providerKey + '&providerKey=' + providetName, { permissions: grantedPermissions })
}