import logging

import azure.functions as func


def main(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request.')

    device = req.params.get('device')
    temperature = req.params.get('temperature')

    if device and temperature:
        import sys
        from sklearn.externals import joblib
        from azure.storage.blob import BlockBlobService
        
        # READ MODEL FROM AZURE BLOB STORAGE
        blob_account = 'akshaystorageaccount1'
        blob_key = '0Z1RtdiCfYhMO7cfD7aSq/rsew7Ry7fhZcarQqTdlpscEYwhFYRjs45h7FboSM5PWehRWGnVNB5hgwkGT4WFlA=='
        blob_container = 'datasciencefilecontainer'
        blob_service = BlockBlobService(account_name=blob_account, account_key=blob_key)
        blob_service.get_blob_to_path(blob_container, 'model_{0}'.format(device), 'model_{0}.model'.format(device))
        model = joblib.load('model_{0}.model'.format(device))

        if -1 == model.predict(temperature):
            logging.info('ALARM! anamoly detected')
            return func.HttpResponse('ALARM! anamoly detected')
        else:
            logging.info('Cool! no anamoly')
            return func.HttpResponse('Cool! no anamoly')

    else:
        return func.HttpResponse(
             "Please pass a device and temperature parameters on the query string or in the request body",
             status_code=400
        )
