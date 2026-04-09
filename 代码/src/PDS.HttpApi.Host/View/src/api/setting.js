import axios from '@/lib/axios-extension';
import url from '@/lib/url-normalize'
export function Pagination(pagination) {
    return axios.get('/api/setting/global/pagination?' + url.normalize(pagination));
}
export function Save(data) {
    return axios.post('/api/setting/global/set', data)
}