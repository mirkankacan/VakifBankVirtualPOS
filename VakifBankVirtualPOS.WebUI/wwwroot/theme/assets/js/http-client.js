// HTTP isteklerini yöneten merkezi fonksiyon
async function makeRequest(url, method = 'GET', data = null) {
    const options = {
        method: method,
        headers: {
            'Content-Type': 'application/json'
        }
    };
    
    // POST, PUT, PATCH, DELETE için CSRF token ekle
    if (['POST', 'PUT', 'PATCH', 'DELETE'].includes(method.toUpperCase())) {
        const csrfToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (csrfToken) {
            options.headers['X-CSRF-TOKEN'] = csrfToken;
        }
    }
    
    // Body ekle
    if (data && ['POST', 'PUT', 'PATCH'].includes(method.toUpperCase())) {
        options.body = JSON.stringify(data);
    }
    
    const response = await fetch(url, options);
    
    if (!response.ok) {
        try {
            const responseText = await response.text();
            const errorData = JSON.parse(responseText);
            
            throw {
                title: errorData.title || 'Hata',
                detail: errorData.detail || errorData.message || `Bir hata oluştu. (HTTP ${response.status})`
            };
        } catch (parseError) {
            if (parseError.title && parseError.detail) {
                throw parseError;
            }
            throw {
                title: 'Hata',
                detail: `Bir hata oluştu. (HTTP ${response.status})`
            };
        }
    }
    
    const contentType = response.headers.get('content-type');
    if (contentType && contentType.includes('application/json')) {
        return await response.json();
    }
    return null;
}